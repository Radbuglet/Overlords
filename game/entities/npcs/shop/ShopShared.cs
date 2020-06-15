using Godot;
using Overlords.game.entities.common;
using Overlords.game.entities.itemStack;
using Overlords.game.entities.player;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.npcs.shop
{
    public class ShopShared : Node, IWorldProvider
    {
        [Export] private int _quantity;
        [Export] private ItemMaterial _material;
        [Export] private int _sellPrice;
        [Export] private int _buyPrice;

        public Node World => this.GetWorldFromEntity();
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void PerformTransaction(Node playerRoot)
        {
            playerRoot.GetBehavior<PlayerShared>().GetInventory().InsertStack(
                this.GetWorldShared().MakeItemStack(_material, _quantity));
        }
    }
}