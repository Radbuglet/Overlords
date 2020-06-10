using Godot;

namespace Overlords.game.entities.common.inventory
{
    public class ItemStack : Node
    {
        [Export] public int Amount;

        public bool IsEmpty()
        {
            return Amount == 0;
        }

        public virtual int GetMaxAmount()
        {
            GD.PushError($"{nameof(GetMaxAmount)} was never overriden by child class.");
            return 64;
        }

        public virtual bool IsSimilar(ItemStack other)
        {
            GD.PushError($"{nameof(IsSimilar)} was never overriden by child class.");
            return false;
        }
    }
}