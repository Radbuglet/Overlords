using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.network.replication
{
    public interface IReplicatorCatchesUp
    {
        IEnumerable<Node> CatchupJoinedPeer(int targetPeerId);
    }
    
    public static class ReplicationMain
    {
        public const string ReceivesCatchupGroup = "replicator_catches_up";

        public static void PerformCatchup(this SceneTree sceneTree, int targetPeerId)  // TODO
        {
            var containerStack = new Stack<(Node container, HashSet<string> allowedChildren)>();
            foreach (var nodeUnCasted in sceneTree.GetNodesInGroup(ReceivesCatchupGroup))
            {
                // Cast node
                var node = (Node) nodeUnCasted;
                if (!(node is IReplicatorCatchesUp catchesUp))
                {
                    GD.PushWarning($"Node not implementing the {nameof(IReplicatorCatchesUp)} interface was part of the group \"{ReceivesCatchupGroup}\".");
                    continue;
                }

                // Check whether the node will be visible in the remote tree or not
                if (containerStack.Count > 0)
                {
                    var pathToContainer = containerStack.Peek().container.GetPathTo(node);
                    if (pathToContainer.GetName(0) == "..")
                    {
                        
                    }
                }

                // Replicate
                catchesUp.CatchupJoinedPeer(targetPeerId);  // TODO: Conditional descendants
            }
        }

        public static void RegisterCatchupReceiver<T>(this T node) where T: Node, IReplicatorCatchesUp
        {
            if (node.GetTree().GetNetworkMode() == NetworkUtils.NetworkMode.Server)
                node.AddToGroup(ReceivesCatchupGroup);
        }
    }
}