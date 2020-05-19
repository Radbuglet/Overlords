using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicServer: Node
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            LogicShared.BalanceValue.Value = (int) GD.RandRange(0, 420.69);
        }

        public PlayerProtocol.NetworkConstructor MakeConstructor(int target)
        {
            return new PlayerProtocol.NetworkConstructor
            {
                OwnerPeerId = LogicShared.OwnerPeerId,
                ReplicatedState = LogicShared.StateReplicator.SerializeValues()
            };
        }
    }
}