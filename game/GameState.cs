using Overlords.helpers.replication;

namespace Overlords.game
{
    public class GameState : StateReplicator
    {
        public readonly ReplicatedField<int?> OverlordId;
        
        public GameState()
        {
            OverlordId = AddField<int?>(isNullable: true);
        }
    }
}