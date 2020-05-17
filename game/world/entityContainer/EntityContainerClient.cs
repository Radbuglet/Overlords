using Godot;

namespace Overlords.game.world.entityContainer
{
    public class EntityContainerClient: Node
    {
        [Signal]
        public delegate void EntityRemotelyAdded();
        
        [Signal]
        public delegate void EntityRemotelyRemoved();
    }
}