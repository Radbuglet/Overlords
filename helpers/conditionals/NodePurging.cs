using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.conditionals
{
    public static class NodePurging  // TODO: What about freeing?
    {
        public static void PurgeParallel(this Node node)
        {
            node.GetParent().RemoveChild(node);
        }
        
        public static void PurgeParallel(IEnumerable<Node> targets)
        {
            foreach (var parallelNode in targets)
            {
                PurgeParallel(parallelNode);
            }
        }
        
        public static void PurgeSelf(this Node node, bool freeSelf)
        {
            foreach (var child in node.GetChildren())
            {
                node.RemoveChild((Node) child);
            }

            if (freeSelf) node.QueueFree();
        }
    }
}