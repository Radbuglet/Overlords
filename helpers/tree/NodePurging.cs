using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.tree
{
    public static class NodePurging  // TODO: What about freeing?
    {
        public static void Purge(this Node node)
        {
            node.GetParent().RemoveChild(node);
            node.QueueFree();
        }
        
        public static void PurgeParallel(IEnumerable<Node> targets)
        {
            foreach (var parallelNode in targets)
            {
                Purge(parallelNode);
            }
        }
        
        public static void PurgeConditionalNode(this Node node, bool freeSelf)
        {
            foreach (var child in node.GetChildren())
            {
                node.RemoveChild((Node) child);
            }

            if (freeSelf) node.QueueFree();
        }
    }
}