using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.helpers.tree
{
    public static class TreeUtils
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

        /// <summary>
        /// Finds the first ancestor that is assignable to the type "T".
        /// Returns null if the operation failed.
        /// </summary>
        /// <returns></returns>
        public static T FindFirstAncestor<T>(this Node node) where T : Node
        {
            foreach (var ancestor in node.EnumerateAncestors())
            {
                if (ancestor is T firstAncestor)
                    return firstAncestor;
            }
            return null;
        }

        /// <summary>
        /// This method is a more memory efficient version of the GetChildren() method present in Nodes because it returns
        /// an iterator instead of creating a new array.
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <returns>An iterator with each child in scene-tree order</returns>
        public static IEnumerable<Node> EnumerateChildren(this Node node)
        {
            for (var idx = 0; idx < node.GetChildCount(); idx++)
            {
                yield return node.GetChild(idx);
            }
        }

        /// <summary>
        /// Gets the first child that has a certain name. This, alongside all other "find node" operations in Godot is
        /// an O(n) operation.
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <param name="name">The expected (exact) name. No regexes supported.</param>
        /// <returns></returns>
        public static Node GetChildByName(this Node node, string name)
        {
            return node.EnumerateChildren().FirstOrDefault(child => child.Name == name);
        }

        /// <summary>
        /// Re-parents all direct children of the "from" node to the "to" node.
        /// </summary>
        public static void MoveInto(this Node from, Node into)
        {
            foreach (var child in from.EnumerateChildren())
                child.ReParent(@into);
        }
        
        public static void ReParent(this Node node, Node newParent)
        {
            node.GetParent()?.RemoveChild(node);
            newParent.AddChild(node);
        }

        /// <summary>
        /// Removes a node from the scene tree immediately and queues the memory for deletion.
        /// </summary>
        public static void Purge(this Node node)
        {
            node.GetParent().RemoveChild(node);
            node.QueueFree();
        }

        
        /// <summary>
        /// Removes all children of a node and optionally queues the parent node for freeing.
        /// This method is useful if the node is locked during a scene tree initialization.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="freeSelf"></param>
        public static void PurgeWhileLocked(this Node node, bool freeSelf)
        {
            foreach (var child in node.EnumerateChildren())
            {
                child.QueueFree();
                node.RemoveChild(child);
            }

            if (freeSelf)
                node.QueueFree();
        }
        
        
        /// <summary>
        /// Lists all the members of a group which derive the generic type "T". Unlike GetNodesInGroup, this method
        /// will account for any dynamic removals during the iteration but not any dynamic additions.
        /// </summary>
        public static IEnumerable<(Node, T)> EnumerateGroupMembers<T>(this SceneTree tree, string groupName)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var node in tree.GetNodesInGroup(groupName).Cast<Node>())
            {
                if (node.IsInGroup(groupName) && node is T)
                    yield return (node, (T) (object) node);
            }
        }
    }
}