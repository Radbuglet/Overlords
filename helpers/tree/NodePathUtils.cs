using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.helpers.tree
{
    public static class NodePathUtils
    {
        public static IEnumerable<Node> EnumerateAncestors(this Node node)
        {
            var ancestor = node.GetParent();
            while (ancestor != null)
            {
                yield return ancestor;
                ancestor = ancestor.GetParent();
            }
        }
        
        public static bool IsDescendantOf(this Node node, Node other)
        {
            return node.EnumerateAncestors().Contains(other);
        }

        public static (bool isDescendant, Node firstNodeAfterAncestor) IsDescendantOfWithBacktrack(this Node node, Node other)
        {
            var firstNodeAfterAncestor = node;
            foreach (var ancestor in node.EnumerateAncestors())
            {
                if (ancestor.Equals(other))
                {
                    return (true, firstNodeAfterAncestor);
                }
                firstNodeAfterAncestor = ancestor;
            }

            return (false, null);
        }
    }
}