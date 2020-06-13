using Godot;
using Overlords.game.entities.itemStack;
using Overlords.game.entities.player;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.shop
{
    public class ShopShared : Node
    {
        [Export] private int _quantity;
        [Export] private ItemMaterial _material;
        [Export] private int _sellPrice;
        [Export] private int _buyPrice;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void PerformTransaction(PlayerShared player)
        {
            player.GetInventory().InsertStack(new ItemStack
            {
                Material = _material,
                Amount = _quantity
            });
        }
    }
}