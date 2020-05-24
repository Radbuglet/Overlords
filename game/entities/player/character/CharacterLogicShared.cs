using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicShared: Node
    {
        public void SetupVariant(NetworkVariantUtils.ObjectVariant variant)
        {
            this.InitializeBehavior();
            this.GetGameObject<Node>().ApplyNetworkVariant(variant,
                null, null, typeof(CharacterLogicLocal), null);
        }
    }
}