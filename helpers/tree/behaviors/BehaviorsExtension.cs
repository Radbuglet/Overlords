using System;
using System.Diagnostics;
using System.Reflection;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.tree.interfaceBehaviors;

namespace Overlords.helpers.tree.behaviors
{
    public static class BehaviorsExtension
    {
        private static void InitializeCommon(this Node initializedNode, Node gameObject)
        {
            var thisType = initializedNode.GetType();
            foreach (var field in thisType.GetRuntimeFields())
            {
                // Behavior linking
                if (field.GetCustomAttribute<RequireBehavior>() != null)
                {
                    Debug.Assert(gameObject != null,
                        "Attempted to use behavior linking in non-behavior initialization!");
                    var requiredBehavior = gameObject.GetBehaviorDynamic(field.FieldType);
                    field.SetValueSafe(initializedNode, requiredBehavior);
                }

                // Parent linking
                if (field.GetCustomAttribute<RequireParent>() != null)
                {
                    var parent = initializedNode.GetParent();
                    Debug.Assert(field.FieldType.IsInstanceOfType(parent),
                        $"Failed to link required parent to node {GetNodeDebugInfo(initializedNode)}: Invalid parent type!");
                    field.SetValueSafe(initializedNode, parent);
                }

                // Node path linking
                if (field.GetValue(initializedNode) == null)
                {
                    var attribLinkNodePath = field.GetCustomAttribute<LinkNodeEditor>();
                    if (attribLinkNodePath != null)
                    {
                        var nodePath = initializedNode.Get(attribLinkNodePath.ExportedPropName);
                        Debug.Assert(nodePath != null,
                            $"Failed to link NodePath: nodePath in field \"{field.Name}\" is null.");
                        Debug.Assert(nodePath is NodePath,
                            $"LinkNodePath attribute placed on non-NodePath field named \"{field.Name}\".");
                        var instance = initializedNode.GetNode((NodePath) nodePath);
                        field.SetValueSafe(initializedNode, instance);
                    }

                    var attribLinkNode = field.GetCustomAttribute<LinkNodeStatic>();
                    if (attribLinkNode != null)
                    {
                        var instance = initializedNode.GetNode(attribLinkNode.Path);
                        field.SetValueSafe(initializedNode, instance);
                    }
                }

                // Field not null (debug only)
                // ReSharper disable scope InvertIf
                Debug.Assert(
                    field.GetCustomAttribute<FieldNotNull>() == null || field.GetValue(initializedNode) != null,
                    $"Field {field.Name} is marked as not allowed to be null during init but is null anyway.");
            }

            foreach (var method in thisType.GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = method.GetCustomAttribute<BindEntitySignal>();
                if (attribute != null)
                {
                    initializedNode.ConnectEntitySignal(attribute.SignalName, method.Name);
                }
            }
        }

        public static void InitializeNode(this Node initializedNode)
        {
            initializedNode.InitializeCommon(null);
        }

        public static void InitializeBehavior(this Node initializedBehavior, bool autoCorrectName)
        {
            if (autoCorrectName)
                initializedBehavior.Name = GetBehaviorNodeName(initializedBehavior.GetType());
            else
                Debug.Assert(initializedBehavior.Name == GetBehaviorNodeName(initializedBehavior.GetType()),
                    $"Behavior node at {initializedBehavior.GetNodeDebugInfo()} must be named \"{GetBehaviorNodeName(initializedBehavior.GetType())}\"");

            var gameObject = initializedBehavior.GetGameObject<Node>();
            initializedBehavior.InitializeCommon(gameObject);
        }

        public static void InitializeBehavior(this Node initializedBehavior)
        {
            initializedBehavior.InitializeBehavior(false);
        }

        private static string GetBehaviorNodeName(Type behaviorType)
        {
            return behaviorType.Name;
        }

        private static string GetNodeDebugInfo(this Node node)
        {
            var path = node.IsInsideTree() ? node.GetPath().ToString() : "{Outside tree}";
            return $"<{node.Name}@{path}>";
        }

        public static TGameObject GetGameObject<TGameObject>(this Node behavior) where TGameObject : Node
        {
            Debug.Assert(behavior != null);
            var gameObject = behavior.GetParentOrNull<TGameObject>();
            Debug.Assert(gameObject != null,
                $"Failed to get GameObject for behavior {behavior.GetNodeDebugInfo()}!");


            return gameObject;
        }

        public static Node GetBehaviorDynamic(this Node gameObject, Type behaviorType, bool required = true)
        {
            Debug.Assert(gameObject != null, "Failed to get behavior for GameObject because it was null!");
            var behavior = gameObject.GetNodeOrNull(GetBehaviorNodeName(behaviorType));
            if (required)
            {
                Debug.Assert(behavior != null,
                $"Failed to get behavior of type \"{behaviorType.Name}\" for GameObject {gameObject.GetNodeDebugInfo()}!");
            }
            if (behavior != null)
            {
                Debug.Assert(behaviorType.IsInstanceOfType(behavior),
                    $"Behavior at {behavior.GetNodeDebugInfo()} isn't of valid type!");
            }
            return behavior;
        }

        public static TBehavior GetBehavior<TBehavior>(this Node gameObject, bool required = true) where TBehavior : Node
        {
            return (TBehavior) GetBehaviorDynamic(gameObject, typeof(TBehavior), required);
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