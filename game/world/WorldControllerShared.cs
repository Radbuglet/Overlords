using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerShared: Node
    {
        public EntityContainer.RegisteredEntityType EntityTypePlayer;
        
        public override void _Ready()
        {
            this.InitializeBehavior();

            var entityContainer = GetNode<EntityContainer>("../../EntityContainer");
            EntityTypePlayer = entityContainer.RegisterEntityType(Protocol.PlayerConstructor.Serializer,
                constructor =>
                {
                    GD.Print("Making player...");
                }, (target, player) => new Protocol.PlayerConstructor());
        }
    }
}