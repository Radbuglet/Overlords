using Overlords.game.world.entityCore;

namespace Overlords.game.entities.player.inventory
{
    public class ItemStackState : StateReplicator
    {
        public readonly ReplicatedField<int> Material;
        public readonly ReplicatedField<int> Amount;

        public ItemStackState()
        {
            Material = AddField<int>(isOneShot: true);  // TODO: Validate values.
            Amount = AddField<int>();
        }
    }
}