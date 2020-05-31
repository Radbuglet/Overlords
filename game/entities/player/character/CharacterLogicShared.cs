using Godot;
using Overlords.game.world;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicShared: Node
    {
        [RequireParent] public KinematicBody Body;
        [LinkNodeStatic("../RemoteEvent")] public RemoteEvent RemoteEvent;
        
        public PlayerLogicShared PlayerShared;

        public void Initialize(PlayerLogicShared playerLogicShared, NetworkTypeUtils.ObjectVariant variant,
            CharacterProtocol.InitialState initialState)
        {
            PlayerShared = playerLogicShared;
            this.InitializeBehavior();
            this.GetGameObject<Node>().ApplyNetworkVariant(variant,
                typeof(CharacterLogicServer), null, typeof(CharacterLogicLocal), typeof(CharacterLogicPuppet));

            Body.Translation = initialState.Position;
            if (variant != NetworkTypeUtils.ObjectVariant.LocalAuthoritative)
            {
                GetNode<Camera>("../FpsCamera").Purge();
            }
        }

        public WorldLogicShared GetWorldShared()
        {
            return PlayerShared.WorldRoot.GetBehavior<WorldLogicShared>();
        }
    }
}