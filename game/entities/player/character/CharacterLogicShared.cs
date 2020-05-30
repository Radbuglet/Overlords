using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicShared: Node
    {
        [LinkNodeStatic("../RemoteEvent")] public RemoteEvent RemoteEvent;
        
        public void Initialize(NetworkUtils.ObjectVariant variant, CharacterProtocol.InitialState initialState)
        {
            this.InitializeBehavior();
            this.GetGameObject<Node>().ApplyNetworkVariant(variant,
                typeof(CharacterLogicServer), null, typeof(CharacterLogicLocal), null);
        }
    }
}