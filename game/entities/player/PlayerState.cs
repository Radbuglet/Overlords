using Overlords.game.world.entityCore;

namespace Overlords.game.entities.player
{
    public class PlayerState : StateReplicator
    {
        public readonly ReplicatedField<int> OwnerPeerId;
        public readonly ReplicatedField<string> DisplayName;
        public readonly ReplicatedField<int> Balance;
        
        public PlayerState()
        {
            OwnerPeerId = AddField<int>(true);
            DisplayName = AddField<string>(true);
            Balance = AddField<int>();
        }
    }
}