using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.helpers.csharp
{
    public static class EnumerableUtils
    {
        public static IEnumerable<Node> ConvertToNodeIterator(this IEnumerable<NodePath> nodePaths, Node root)
        {
            return nodePaths.Select(root.GetNodeOrNull).Where(node => node != null);
        }

        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            yield return value;
        }
    }
}