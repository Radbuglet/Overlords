using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.tree.interfaceBehaviors
{
    public class InterfaceBehaviorRouter : Node
    {
        private readonly Dictionary<Type, object> _implementations = new Dictionary<Type, object>();

        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void DeclareImplementation(Type iface, object impl)
        {
            Debug.Assert(iface.IsInstanceOfType(impl));
            Debug.Assert(!_implementations.ContainsKey(iface));
            _implementations.Add(iface, impl);
        }

        public TInterface GetImplementation<TInterface>()
        {
            var gotImplementation = _implementations.TryGetValue(typeof(TInterface), out var implementation);
            Debug.Assert(gotImplementation);
            return (TInterface) implementation;
        }
    }

    public static class InterfaceBehaviorExtensions
    {
        public static void DeclareImplementation(this Node impl, IEnumerable<Type> interfaces)
        {
            var router = impl.GetGameObject<Node>().GetBehavior<InterfaceBehaviorRouter>();
            foreach (var iface in interfaces) router.DeclareImplementation(iface, impl);
        }

        public static TImpl GetImplementation<TImpl>(this Node root)
        {
            return root.GetBehavior<InterfaceBehaviorRouter>().GetImplementation<TImpl>();
        }
    }
}