using Overlords.helpers.replication;

namespace Overlords.game.player.inventory
{
    public class ItemStackState : StateReplicator
    {
        public readonly ReplicatedField<int> Material;
        public readonly ReplicatedField<int> Amount;

        public ItemStackState()
        {
            Material = AddField<int>(true);  // TODO: Validate values.
            Amount = AddField<int>();
        }
    }
}