using Godot;
using Overlords.game.world;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.common
{
    public interface IWorldProvider
    {
        Node World { get; }
    }

    public static class WorldReferencingUtils
    {
        public static Node GetWorldFromEntity(this Node entityRoot)
        {
            return entityRoot.GetParent().GetParent().GetParent();
        }
        
        public static WorldShared GetWorldShared(this IWorldProvider self)
        {
            return self.World.GetBehavior<WorldShared>();
        }
        
        public static WorldClient GetWorldClient(this IWorldProvider self)
        {
            return self.World.GetBehavior<WorldClient>();
        }
        
        public static WorldServer GetWorldServer(this IWorldProvider self)
        {
            return self.World.GetBehavior<WorldServer>();
        }
    }
}