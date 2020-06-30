using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Overlords.helpers.network
{
    /// <summary>
    /// Represents the packet that will be delivered to the remote instance upon catch up. Alongside the remote argument data,
    /// this object also contains information about whether or not children should replicate themselves. This is useful for
    /// network culling.
    /// </summary>
    public readonly struct CatchupState
    {
        [Flags]
        public enum CullMode
        {
            SendInfo = 0x_0000_0010,
            SendChildren = 0x_0000_0001
        }
        
        public readonly CullMode Mode;
        public readonly object RemoteArgs;
        
        public CatchupState(object remoteArgs, bool sendChildren)
        {
            Mode = CullMode.SendInfo | (sendChildren ? CullMode.SendChildren : 0);
            RemoteArgs = remoteArgs;
        }
        
        public CatchupState(bool sendChildren)
        {
            Mode = sendChildren ? CullMode.SendChildren : 0;
            RemoteArgs = null;
        }
    }

    /// <summary>
    /// Represents an exception signaling a fatal error in the received catch up state.
    /// </summary>
    public class InvalidCatchupException : Exception
    {
        public Node Instigator;
        
        public InvalidCatchupException(string reason) : base(reason)
        {
        }

        public string ToMessage()
        {
            return $"{(Instigator != null ? Instigator.GetPath().ToString() : "Unknown source")} detected a fatal issue in the received catch up data: {Message}";
        }
    }
    
    /// <summary>
    /// Represents a node that should have its initial state replicated to new peers (i.e. catching up).
    /// `CatchupOverNetwork` is in charge of generating the data whereas `HandleCatchupState` is in charge of applying it
    /// with micro level validation (validation for this node only).
    /// `CatchupOverNetwork` must contain HLAPI serializable objects (i.e. objects descending from variant that can be
    /// marshalled).
    /// `HandleCatchupState` can be called several times remotely. TODO: Fix this problem using auto unflagging groups
    /// </summary>
    public interface IRequiresCatchup
    {
        CatchupState CatchupOverNetwork(int peerId);
        void HandleCatchupState(object argsRoot);
    }

    /// <summary>
    /// Represents a node that enforces certain rules about the macro level state of the tree when getting caught up by a
    /// remote source.
    /// These nodes wishing to enforce invariants must flag themselves explicitly using the `CatchupExtensions.FlagEnforcer()`
    /// extension method and this flag will be automatically removed every time the state gets validated.
    /// In order to signal an unrecoverable error in the received state, an `InvalidCatchupException` must be raised.
    /// </summary>
    public interface IInvariantEnforcer
    {
        void ValidateCatchupState(SceneTree tree);
    }

    /// <summary>
    /// Represents a node that is waiting for all state to be validated before running startup logic.
    /// The nodes wishing to receive this notification must flag themselves explicitly using the `CatchupExtensions.FlagAwaiter`
    /// extension method and this flag will be automatically removed every time the state is validated.
    /// </summary>
    public interface IValidationAwaiter
    {
        void _StateValidated();
    }
    
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public static class CatchupExtensions
    {
        private const string GroupInvariantEnforcer = "enforces_invariants";
        private const string GroupValidationAwaiter = "awaits_validation";
        
        /// <summary>
        /// Produces a single dictionary containing which the remote instance can then use to catch up their simulation
        /// using `ApplyCatchupInfo`. Only the nodes that are descendants of the rootNode will be included in the packet
        /// and some nodes may be omitted if the node declares that children should be hidden.
        /// </summary>
        /// <param name="rootNode">The node at the root of the replication.</param>
        /// <param name="peerId">
        /// The peer id for which the packet is generated for. Knowledge of the peer id helps with network culling
        /// </param>
        /// <returns>A single dictionary which can be sent over the network.</returns>
        public static Dictionary GenerateCatchupInfo(this Node rootNode, int peerId)
        {
            var packet = new Dictionary();

            // TODO: Optimize using groups, check impact of NodePath serialization.
            void HandleNode(Node node)
            {
                // Serialize the node
                if (node is IRequiresCatchup requiresCatchup)
                {
                    var generated = requiresCatchup.CatchupOverNetwork(peerId);
                    if ((generated.Mode & CatchupState.CullMode.SendInfo) != 0)
                    {
                        packet[node.GetPath()] = generated.RemoteArgs;
                    }
                    if ((generated.Mode & CatchupState.CullMode.SendChildren) == 0)
                    {
                        return;  // Children will not be sent.
                    }
                }
                
                // Traverse the branch if `CatchupOverNetwork` returned true
                foreach (var child in node.GetChildren().Cast<Node>())
                {
                    HandleNode(child);
                }
            }
            HandleNode(rootNode);
            
            return packet;
        }

        /// <summary>
        /// Runs `HandleCatchupState` on any instances who have received catchup information. Not all instances implementing
        /// `IRequiresCatchup` will receive this information and any instance implementing the interface could be subject
        /// to getting `HandleCatchupState` called.  TODO: Allow user to limit who can receive catchup for easier error resolution
        /// 
        /// Invariant enforcers and validation awaiters can be registered during this time. However, invariant enforcers
        /// can't register new invariant enforcers and validation awaiters can't register new validation awaiters.
        ///  
        /// Any nodes flagged as requiring invariant enforcement using the `FlagEnforcer` method will be handled any any
        /// error thrown by them will be returned by this method. Any nodes marked as enforcers will have this flag removed
        /// automatically by the method unless the method returned a non-null result, in which case, only some of the
        /// enforcers may have had this flag removed.
        /// 
        /// Any node awaiting the end of validation will be notified and their flag will be removed as well.
        /// 
        /// </summary>
        /// <param name="tree">The SceneTree on which the catchup is performed.</param>
        /// <param name="data">The packet remotely generated by `GenerateCatchupInfo`</param>
        /// <returns>
        /// An InvalidCatchupException with their `Instigator` field set to the node implementing `IInvariantEnforcer`
        /// that raised the exception if a fatal invalid state error has been raised or null if everything went well.
        /// </returns>
        public static InvalidCatchupException ApplyCatchupInfo(this SceneTree tree, Dictionary data)
        {
            const string errorPrefix = "Catchup info target entry is malformed and was ignored: ";

            // Deliver catchup "RPCs"
            foreach (var pathRaw in data.Keys)
            {
                // Parse path
                if (!(pathRaw is NodePath path))
                {
                    GD.PushWarning(errorPrefix + "key was not a node path.");
                    continue;
                }
                var node = tree.Root.GetNodeOrNull<IRequiresCatchup>(path);
                if (node == null)
                {
                    GD.PushWarning(errorPrefix + $"specified node does not exist or does not implement {nameof(IRequiresCatchup)}.");
                    continue;
                }
                
                node.HandleCatchupState(data[pathRaw]);
            }
            
            // Check invariants
            IEnumerable<(Node, TInterface)> GetNodesInGroup<TInterface>(string groupName) where TInterface : class
            {
                return tree.GetNodesInGroup(groupName).Cast<Node>()
                    .Where(node => node.IsInsideTree() && node.IsInGroup(groupName))
                    .Select(node => (node, node as TInterface));
            }

            foreach (var (node, enforcer) in GetNodesInGroup<IInvariantEnforcer>(GroupInvariantEnforcer))
            {
                try
                {
                    enforcer.ValidateCatchupState(tree);
                }
                catch (InvalidCatchupException e)
                {
                    e.Instigator = node;
                    return e;
                }
                node.RemoveFromGroup(GroupInvariantEnforcer);
            }

            // Notify validation finish
            foreach (var (node, awaiter) in GetNodesInGroup<IValidationAwaiter>(GroupValidationAwaiter))
            {
                awaiter._StateValidated();
                node.RemoveFromGroup(GroupValidationAwaiter);
            }

            return null;
        }

        /// <summary>
        /// Flags an invariant enforcer as needing to enforce invariants. See `IInvariantEnforcer` for more details.
        /// NOTE: new enforcers cannot be registered during the enforcement phase as they will be ignored by `ApplyCatchupInfo`.
        /// </summary>
        /// <param name="node">The invariant enforcer to be flagged</param>
        public static void FlagEnforcer<T>(this T node) where T: Node, IInvariantEnforcer
        {
            node.AddToGroup(GroupInvariantEnforcer);
        }
        
        /// <summary>
        /// Flags a validation awaiter as needing to wait for validation. See `IValidationAwaiter` for more details.
        /// NOTE: new awaiters cannot be registered during the await resolution phase as they will be ignored by `ApplyCatchupInfo`.
        /// </summary>
        /// <param name="node">The validation awaiter to be flagged</param>
        /// <typeparam name="T"></typeparam>
        public static void FlagAwaiter<T>(this T node) where T: Node, IValidationAwaiter
        {
            node.AddToGroup(GroupValidationAwaiter);
        }
    }
}