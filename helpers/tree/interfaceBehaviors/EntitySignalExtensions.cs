using Godot;
using Godot.Collections;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.tree.interfaceBehaviors
{
    public static class EntitySignalExtensions
    {
        public static void ConnectEntitySignal(this Node behavior, string name, string handlerName, Array binds = null)
        {
            var gameObject = behavior.GetGameObject<Node>();
            if (!gameObject.HasUserSignal(name))
            {
                gameObject.AddUserSignal(name);
            }
            gameObject.Connect(name, behavior, handlerName, binds);
        }

        public static void FireEntitySignal(this Node entityRoot, string signalName, params object[] args)
        {
            entityRoot.EmitSignal(signalName, args);
        }
    }
}