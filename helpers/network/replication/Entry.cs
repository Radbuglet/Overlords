using System.Collections.Generic;
using Godot;
using Overlords.helpers.tree;

namespace Overlords.helpers.network.replication
{
    public interface IReplicatorCatchesUp
    {
        IEnumerable<Node> CatchupJoinedPeer(int targetPeerId);
    }
    
    public static class ReplicationMain
    {
        public const string ReceivesCatchupGroup = "replicator_catches_up";

        public static void PerformCatchup(this SceneTree sceneTree, int targetPeerId)
        {
            var containerStack = new Stack<(Node container, HashSet<string> allowedChildren)>();
            foreach (var nodeUnCasted in sceneTree.GetNodesInGroup(ReceivesCatchupGroup))  // TODO: Better documentation; test
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
                    // Backtrack to first container that contains this node
                    var isContained = false;
                    Node containedBy = null;
                    while (!isContained)
                    {
                        (isContained, containedBy) = node.IsDescendantOfWithBacktrack(containerStack.Peek().container);
                        containerStack.Pop();
                        if (containerStack.Count == 0) break;
                    }

                    // Check if we're allowed to replicate
                    if (containerStack.Count > 0 && !containerStack.Peek().allowedChildren.Contains(containedBy.Name))
                        continue;
                }

                // Replicate and update the container rules
                var allowedContainers = catchesUp.CatchupJoinedPeer(targetPeerId);
                if (allowedContainers == null) continue;  // No additional restrictions have been added.
                var allowedNames = new HashSet<string>();
                foreach (var allowedContainer in allowedContainers)
                {
                    allowedNames.Add(allowedContainer.Name);
                }
                containerStack.Push((node, allowedNames));
            }
        }

        public static void RegisterCatchupReceiver<T>(this T node) where T: Node, IReplicatorCatchesUp
        {
            if (node.GetTree().GetNetworkMode() == NetworkUtils.NetworkMode.Server)
                node.AddToGroup(ReceivesCatchupGroup);
        }
    }
}