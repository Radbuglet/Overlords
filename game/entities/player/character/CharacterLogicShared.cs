using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicShared: Node
    {
        [LinkNodeStatic("../RemoteEvent")] public RemoteEvent RemoteEvent;
        
        public void SetupVariant(NetworkVariantUtils.ObjectVariant variant)
        {
            this.InitializeBehavior();
            this.GetGameObject<Node>().ApplyNetworkVariant(variant,
                typeof(CharacterLogicServer), null, typeof(CharacterLogicLocal), null);
        }
    }
}