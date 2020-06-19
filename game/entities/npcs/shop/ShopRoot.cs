using Godot;
using Overlords.game.definitions;

namespace Overlords.game.entities.npcs.shop
{
    public class ShopRoot : StaticBody
    {
        public override void _Ready()
        {
            AddToGroup(EntityTypes.InteractableGroupName);
        }
    }
}