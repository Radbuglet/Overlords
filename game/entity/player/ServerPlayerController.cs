using Godot;
using Overlords.helpers.behaviors;

namespace Overlords.game.entity.player
{
    public class ServerPlayerController: Node
    {
        public int OwnerPeerId;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}