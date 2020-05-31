using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.helpers.tree
{
    public static class NodeUtils
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

        public static (bool isDescendant, Node firstNodeAfterAncestor) IsDescendantOfWithBacktrack(this Node node,
            Node other)
        {
            var firstNodeAfterAncestor = node;
            foreach (var ancestor in node.EnumerateAncestors())
            {
                if (ancestor.Equals(other)) return (true, firstNodeAfterAncestor);
                firstNodeAfterAncestor = ancestor;
            }

            return (false, null);
        }

        public static Node GetChildByName(this Node node, string name)
        {
            return node.EnumerateChildren().FirstOrDefault(child => child.Name == name);
        }

        public static void MoveInto(this Node from, Node into)
        {
            foreach (var child in from.EnumerateChildren()) child.ReParent(@into);
        }

        public static void ImportNodesFrom(this Node into, PackedScene from)
        {
            from.Instance().MoveInto(into);
        }

        public static void ReParent(this Node node, Node newParent)
        {
            node.GetParent()?.RemoveChild(node);
            newParent.AddChild(node);
        }

        public static IEnumerable<Node> EnumerateChildren(this Node node)
        {
            return node.GetChildren().Cast<Node>();
        }
    }
}