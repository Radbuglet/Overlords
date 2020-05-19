using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicLocal: Node
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            GD.Print("Hi, I'm a client! ", LogicShared.BalanceValue.Value);
        }
    }
}