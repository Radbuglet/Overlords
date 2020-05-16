using System;
using Godot;

namespace Overlords.helpers.tree.behaviors
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNodeStatic: Attribute
    {
        public readonly NodePath Path;
        public LinkNodeStatic(string path)
        {
            Path = path;
        }
    }
}