using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.helpers.csharp
{
    public static class IteratorUtils
    {
        public static IEnumerable<Node> ConvertToNodeIterator(this IEnumerable<NodePath> nodePaths, Node root, bool ignoreMissing = false)
        {
            return nodePaths.Select(path => ignoreMissing ?
                root.GetNodeOrNull(path) : root.GetNode(path)).Where(node => node != null);
        }

        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            yield return value;
        }
    }
}