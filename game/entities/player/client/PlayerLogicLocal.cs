using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.client
{
    public class PlayerLogicLocal: Node
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}