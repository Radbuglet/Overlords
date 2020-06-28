using System;
using System.Diagnostics;
using System.Reflection;
using Godot;
using Overlords.helpers.csharp;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.tree
{
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
    
    public static class InitializationExtensions
    {
        public static void Initialize(this Node initializedNode)
        {
            var thisType = initializedNode.GetType();
            foreach (var field in thisType.GetRuntimeFields())
            {
                // Node path linking
                if (field.GetValue(initializedNode) == null)
                {
                    // Through editor node paths
                    var attribLinkNodePath = field.GetCustomAttribute<LinkNodeEditor>();
                    if (attribLinkNodePath != null)
                    {
                        var nodePath = initializedNode.Get(attribLinkNodePath.ExportedPropName);
                        Debug.Assert(nodePath != null,
                            $"Failed to link NodePath: nodePath in field \"{field.Name}\" is null.");
                        Debug.Assert(nodePath is NodePath,
                            $"LinkNodePath attribute placed on non-NodePath field named \"{field.Name}\".");
                        var instance = initializedNode.GetNode((NodePath) nodePath);
                        Debug.Assert(instance != null, $"Failed to get dynamically linked node at \"{nodePath}\".");
                        field.SetValueOrFail(initializedNode, instance);
                    }

                    // Through static node pats
                    var attribLinkNode = field.GetCustomAttribute<LinkNodeStatic>();
                    if (attribLinkNode != null)
                    {
                        var instance = initializedNode.GetNode(attribLinkNode.Path);
                        Debug.Assert(instance != null, $"Failed to get statically linked node at \"{attribLinkNode.Path}\".");
                        field.SetValueOrFail(initializedNode, instance);
                    }
                }

                // Field not null (debug only)
                Debug.Assert(
                    field.GetCustomAttribute<FieldNotNull>() == null || field.GetValue(initializedNode) != null,
                    $"Field {field.Name} is marked as not allowed to be null during init but is null anyway.");
            }
        }

        public static void AddUserSignals(this Node node)
        {
            var type = node.GetType();
            foreach (var delegateMaybe in type.GetNestedTypes())
                if (delegateMaybe.GetCustomAttribute<SignalAttribute>() != null)
                {
                    Debug.Assert(!node.HasUserSignal(delegateMaybe.Name));
                    node.AddUserSignal(delegateMaybe.Name);
                }
        }
    }
}