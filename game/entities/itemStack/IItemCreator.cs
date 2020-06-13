namespace Overlords.game.entities.itemStack
{
    public interface IItemCreator
    {
        ItemStack MakeNormalStack(ItemMaterial material, int amount);
    }
}