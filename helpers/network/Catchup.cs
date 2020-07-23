using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Godot;
using Godot.Collections;
using Overlords.helpers.tree;

// TODO: Document
namespace Overlords.helpers.network
{
    /// <summary>
    /// Represents an exception signaling a fatal error in the received catch up state.
    /// </summary>
    public class InvalidCatchupException : Exception
    {
        public Node Instigator;
        
        public InvalidCatchupException(string reason) : base(reason)
        {
        }

        public string GetMessage()
        {
            return $"{(Instigator != null ? Instigator.GetPath().ToString() : "Unspecified source")} detected a fatal issue in the received catch up data: {Message}";
        }
    }

    public interface IRequiresCatchup
    {
        object CatchupOverNetwork(int peerId);
        void HandleCatchupState(object argsRoot);
    }
    
    public interface IMacroCatchupValidator
    {
        void ValidateCatchupState();
    }

    public interface ICatchupAwaiter
    {
        void _CaughtUp();
    }
    
    public static class CatchupExtensions
    {
        private enum CatchupApplicationPhase
        {
            NotCatchingUp,
            ApplyingInfo,
            SecondPass
        }
        
        private const string GroupRequiresCatchup = "requires_catchup";
        private const string GroupMacroCatchupValidator = "macro_catchup_validator";
        private const string GroupCatchupAwaiter = "catchup_awaiter";
        private static CatchupApplicationPhase _catchupPhase;
        
        // TODO: Implement custom packet serialization
        public static Dictionary GenerateCatchupInfo(this Node rootNode, int peerId)
        {
            var packet = new Dictionary();
            Debug.Assert(rootNode.IsVisibleTo(peerId));
            
            void HandleNode(Node node)
            {
                // Check if this node is visible
                if (node is ICullsNetwork cullsNetwork && !cullsNetwork.IsLocallyVisibleTo(peerId))
                    return;

                // Serialize the node
                if (node is IRequiresCatchup requiresCatchup)
                {
                    packet[node.GetPath()] = requiresCatchup.CatchupOverNetwork(peerId);
                }
                
                // Traverse the branch if `CatchupOverNetwork` returned true
                foreach (var child in node.EnumerateChildren())
                {
                    HandleNode(child);
                }
            }
            HandleNode(rootNode);
            
            return packet;
        }
        
        public static InvalidCatchupException ApplyCatchupInfo(this SceneTree tree, Dictionary data)
        {
            const string errorPrefix = "Catchup info target entry is malformed and was ignored: ";
            Debug.Assert(_catchupPhase == CatchupApplicationPhase.NotCatchingUp);

            // Deliver catchup "RPCs"
            _catchupPhase = CatchupApplicationPhase.ApplyingInfo;
            foreach (var pathRaw in data.Keys)
            {
                // Parse path
                if (!(pathRaw is NodePath path))
                {
                    GD.PushWarning(errorPrefix + "key was not a node path.");
                    continue;
                }

                GD.Print($"Catching up node at \"{path}\"");

                var node = tree.Root.GetNodeOrNull<IRequiresCatchup>(path);
                if (node == null)
                {
                    GD.PushWarning(errorPrefix +
                                   $"specified node does not exist or does not implement {nameof(IRequiresCatchup)}.");
                    continue;
                }

                if (!((Node) node).IsInGroup(GroupRequiresCatchup))
                {
                    GD.PushWarning(errorPrefix + "specified node at is not flagged as currently requiring catchup.");
                    continue;
                }

                // Attempt to apply catchup
                try
                {
                    node.HandleCatchupState(data[pathRaw]);
                    ((Node) node).RemoveFromGroup(GroupRequiresCatchup);
                }
                catch (InvalidCatchupException e)
                {
                    e.Instigator = (Node) node;
                    _catchupPhase = CatchupApplicationPhase.NotCatchingUp;
                    return e;
                }
            }

            // Ensure that all nodes requiring catchup have been caught up
            if (tree.DoesAnythingRequireCatchup())
            {
                _catchupPhase = CatchupApplicationPhase.NotCatchingUp;
                var list = new StringBuilder();
                var first = true;
                foreach (var node in tree.GetNodesInGroup(GroupRequiresCatchup).Cast<Node>())
                {
                    if (!first)
                        list.Append(",");
                    first = false;
                    list.Append(node.GetPath());
                }
                return new InvalidCatchupException($"At least one node marked as currently requiring catchup has not been caught up.\nNodes: {list}");
            }

            // Perform macro catchup validation
            _catchupPhase = CatchupApplicationPhase.SecondPass;
            foreach (var (node, validator) in tree.EnumerateGroupMembers<IMacroCatchupValidator>(GroupMacroCatchupValidator))
            {
                try
                {
                    validator.ValidateCatchupState();
                }
                catch (InvalidCatchupException e)
                {
                    e.Instigator = node;
                    _catchupPhase = CatchupApplicationPhase.NotCatchingUp;
                    return e;
                }

                node.RemoveFromGroup(GroupMacroCatchupValidator);
            }
            
            // Announce catchup finish
            foreach (var (node, awaiter) in tree.EnumerateGroupMembers<ICatchupAwaiter>(GroupCatchupAwaiter))
            {
                awaiter._CaughtUp();
                node.RemoveFromGroup(GroupCatchupAwaiter);
            }
            
            _catchupPhase = CatchupApplicationPhase.NotCatchingUp;
            return null;
        }

        // Flagging methods
        public static bool DoesAnythingRequireCatchup(this SceneTree tree)
        {
            return tree.GetNodesInGroup(GroupRequiresCatchup).Count != 0;
        }
        
        public static void FlagRequiresCatchup<T>(this T target) where T : Node, IRequiresCatchup
        {
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.AddToGroup(GroupRequiresCatchup);
        }
        
        public static void UnFlagRequiresCatchup(this Node target)
        {
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.RemoveFromGroup(GroupRequiresCatchup);
        }

        public static bool DoesRequireCatchup(this Node target)
        {
            return target.IsInGroup(GroupRequiresCatchup);
        }
        
        
        public static void FlagMacroValidator<T>(this T target) where T : Node, IMacroCatchupValidator
        {
            Debug.Assert(_catchupPhase != CatchupApplicationPhase.SecondPass);
            
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.AddToGroup(GroupMacroCatchupValidator);
        }
        
        public static void UnFlagMacroValidator(this Node target)
        {
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.RemoveFromGroup(GroupMacroCatchupValidator);
        }
        
        
        public static void FlagCatchupAwaiter<T>(this T target) where T : Node, ICatchupAwaiter
        {
            Debug.Assert(_catchupPhase != CatchupApplicationPhase.SecondPass);
            
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.AddToGroup(GroupCatchupAwaiter);
        }
        
        public static void UnFlagCatchupAwaiter(this Node target)
        {
            if (target.GetNetworkMode() == NetworkMode.Client)
                target.RemoveFromGroup(GroupCatchupAwaiter);
        }
    }
}