using Godot;
using Overlords.helpers.network.replication;
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

            LogicShared.BalanceValue.Connect(nameof(StateField<int>.ValueChangedRemotely), this, nameof(_BalanceChangedRemotely));
        }

        private void _BalanceChangedRemotely(int newBalance, int oldBalance)
        {
            GD.Print("Changed balance to ", newBalance);
        }
    }
}