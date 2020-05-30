using System.Diagnostics;
using Godot;
using Overlords.game.entities.player.character;
using Overlords.game.entities.player.client;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared: Node
    {
        [FieldNotNull] [Export] private PackedScene _characterPrefab;
        
        [RequireParent] public Spatial PlayerRoot;

        public int OwnerPeerId;
        public Node WorldRoot;
        
        public Node CharacterRoot;
        public int Balance;

        public void Initialize(SceneTree tree, Node worldRoot, int ownerPeerId, PlayerProtocol.InitialState initialState)
        {
            this.InitializeBehavior();
            var networkVariant = tree.GetNetworkVariant(ownerPeerId);
            PlayerRoot.Name = $"player_{ownerPeerId}";
            WorldRoot = worldRoot;
            OwnerPeerId = ownerPeerId;

            // Setup variant
            this.GetGameObject<Node>().ApplyNetworkVariant(networkVariant, typeof(PlayerLogicServer),
                typeof(PlayerLogicClient), typeof(PlayerLogicLocal), typeof(PlayerLogicPuppet));

            // Setup shared
            Balance = initialState.Balance;
            if (initialState.CharacterState != null)
            {
                BuildCharacter(networkVariant, initialState.CharacterState);
            }
        }

        public void BuildCharacter(NetworkUtils.ObjectVariant variant, CharacterProtocol.InitialState initialState)
        {
            Debug.Assert(CharacterRoot == null);
            CharacterRoot = _characterPrefab.Instance();
            CharacterRoot.GetBehavior<CharacterLogicShared>().Initialize(variant, initialState);
            AddChild(CharacterRoot);
        }

        public void DestroyCharacter()
        {
            Debug.Assert(CharacterRoot != null);
            CharacterRoot.Purge();
            CharacterRoot = null;
        }

        public bool HasCharacter()
        {
            return CharacterRoot != null;
        }
    }
}