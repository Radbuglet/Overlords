using Godot;

namespace Overlords.game.entities.itemStack
{
    public class ItemStack : Node
    {
        [Export] public ItemMaterial Material;
        [Export] public int Amount;

        public bool IsEmpty()
        {
            return Amount == 0;
        }

        public int GetMaxAmount()
        {
            return 64;
        }

        public bool IsSimilar(ItemStack other)
        {
            return other.Material == Material;
        }
    }
}