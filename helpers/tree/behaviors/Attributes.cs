using System;
using Godot;

namespace Overlords.helpers.tree.behaviors
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BindEntitySignal : Attribute
    {
        public readonly string SignalName;

        public BindEntitySignal(string signalName)
        {
            SignalName = signalName;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FieldNotNull : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNodeEditor : Attribute
    {
        public readonly string ExportedPropName;

        public LinkNodeEditor(string exportedPropName)
        {
            ExportedPropName = exportedPropName;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkNodeStatic : Attribute
    {
        public readonly NodePath Path;

        public LinkNodeStatic(string path)
        {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RequireBehavior : Attribute
    {
        
    }
}