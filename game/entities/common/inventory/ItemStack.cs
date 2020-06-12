using Godot;

namespace Overlords.game.entities.common.inventory
{
    public abstract class ItemStack : Node
    {
        [Export] public int Amount;

        public bool IsEmpty()
        {
            return Amount == 0;
        }

        public abstract int GetMaxAmount();
        public abstract bool IsSimilar(ItemStack other);
    }
}