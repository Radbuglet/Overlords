using Godot;
using Overlords.helpers.behaviors;

namespace Overlords.game.entity.player
{
    public class ServerPlayerController: Node
    {
        private int _ownerPeerId;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void Configure(int ownerPeerId)
        {
            _ownerPeerId = ownerPeerId;
        }
    }
}