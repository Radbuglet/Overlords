using System;
using Godot;

namespace Overlords.helpers.tree.initialization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BindParentSignal : Attribute
    {
        public readonly string SignalName;

        public BindParentSignal(string signalName)
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
}