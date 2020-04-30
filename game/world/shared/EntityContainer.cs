using System.Diagnostics;
using Godot;

namespace Overlords.game.world.shared
{
    public class EntityContainer: Node
    {
        public void AddEntity(string networkId, Node instance)
        {
            Debug.Assert(!HasNode(networkId), "AddEntity() was provided a networkId that is already in use!");
            instance.Name = networkId;
            AddChild(instance);
        }

        public void FreeEntity(Node instance, bool immediate)
        {
            if (immediate) instance.Free(); else instance.QueueFree();
        }
    }
}