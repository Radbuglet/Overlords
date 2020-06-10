using Godot;
using Overlords.game.world;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.common
{
    public interface IWorldReferencer
    {
        Node World { get; }
    }

    public static class WorldReferencingUtils
    {
        public static WorldShared GetWorldShared(this IWorldReferencer self)
        {
            return self.World.GetBehavior<WorldShared>();
        }
        
        public static WorldClient GetWorldClient(this IWorldReferencer self)
        {
            return self.World.GetBehavior<WorldClient>();
        }
    }
}