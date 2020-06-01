using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.tree
{
    public static class NodePurging
    {
        public static void Purge(this Node node)
        {
            node.GetParent().RemoveChild(node);
            node.QueueFree();
        }

        public static void PurgeParallel(IEnumerable<Node> targets)
        {
            foreach (var parallelNode in targets) Purge(parallelNode);
        }

        public static void PurgeWhileLocked(this Node node, bool freeSelf)
        {
            foreach (var child in node.EnumerateChildren()) node.RemoveChild(child);

            if (freeSelf) node.QueueFree();
        }
    }
}