using System;
using Godot;

namespace Overlords.helpers.tree.behaviors
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNode: Attribute
    {
        public readonly NodePath Path;
        public LinkNode(string path)
        {
            Path = path;
        }
    }
}