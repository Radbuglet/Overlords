using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerShared: Node
    {
        [LinkNodeStatic("../../EntityContainer")]
        public EntityContainer EntityContainer;
        
        public EntityContainer.RegisteredEntityType EntityTypePlayer;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityTypePlayer = EntityContainer.RegisterEntityType(Protocol.PlayerConstructor.Serializer,
                constructor =>
                {
                    GD.Print("Making player...");
                }, (target, player) => new Protocol.PlayerConstructor());
        }
    }
}