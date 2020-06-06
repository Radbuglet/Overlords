using Godot;
using Overlords.game.world;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicShared : Node
    {
        public KinematicBody Body => this.GetGameObject<KinematicBody>();
        [LinkNodeStatic("../RemoteEvent")] public RemoteEvent RemoteEvent;
        public Node PlayerRoot;
        public WorldLogicShared WorldShared => PlayerRoot.GetBehavior<PlayerLogicShared>().WorldRoot.GetBehavior<WorldLogicShared>();
        public const float InteractDistance = 6;

        public void Initialize(Node playerRoot, NetworkTypeUtils.ObjectVariant variant,
            CharacterProtocol.InitialState initialState)
        {
            PlayerRoot = playerRoot;
            this.InitializeBehavior();
            this.GetGameObject<Node>().ApplyNetworkVariant(variant,
                typeof(CharacterLogicServer), null, typeof(CharacterLogicLocal), typeof(CharacterLogicPuppet));

            Body.Translation = initialState.Position;
            if (variant != NetworkTypeUtils.ObjectVariant.LocalAuthoritative)
                GetNode<Control>("../FpsCamera/Hud").Purge();
        }
    }
}