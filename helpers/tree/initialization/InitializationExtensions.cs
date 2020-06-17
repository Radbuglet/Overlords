using System.Diagnostics;
using System.Reflection;
using Godot;
using Overlords.helpers.csharp;

namespace Overlords.helpers.tree.initialization
{
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
                        field.SetValueSafe(initializedNode, instance);
                    }

                    // Through static node pats
                    var attribLinkNode = field.GetCustomAttribute<LinkNodeStatic>();
                    if (attribLinkNode != null)
                    {
                        var instance = initializedNode.GetNode(attribLinkNode.Path);
                        Debug.Assert(instance != null, $"Failed to get statically linked node at \"{attribLinkNode.Path}\".");
                        field.SetValueSafe(initializedNode, instance);
                    }
                }

                // Field not null (debug only)
                Debug.Assert(
                    field.GetCustomAttribute<FieldNotNull>() == null || field.GetValue(initializedNode) != null,
                    $"Field {field.Name} is marked as not allowed to be null during init but is null anyway.");
            }

            // Bind parent signals
            var parent = initializedNode.GetParent();
            foreach (var method in thisType.GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = method.GetCustomAttribute<BindParentSignal>();
                if (attribute != null)
                {
                    parent.ConnectOrCreate(attribute.SignalName, initializedNode, method.Name);
                }
            }
        }

        public static void ConnectOrCreate(this Node provider, string signalName, Node target, string methodName)
        {
            if (!provider.HasSignal(signalName))
                provider.AddUserSignal(signalName);
            provider.Connect(signalName, target, methodName);
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