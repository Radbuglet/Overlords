using Overlords.helpers.replication;

namespace Overlords.game.player.inventory
{
    public class ItemStackState : StateConstructor
    {
        public int Material;
        public int Amount;

        public ItemStackState()
        {
            // TODO: Validate values.
            AddField(() => Material, v => Material = v);
            AddField(() => Amount, v => Amount = v);
        }
    }
}