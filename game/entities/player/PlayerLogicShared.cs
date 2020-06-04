using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.game.entities.player.character;
using Overlords.game.entities.player.client;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared : Node
    {
        [RequireParent] public Spatial PlayerRoot;
        [LinkNodeStatic("../StateReplicator")] public StateReplicator StateReplicator;
        [FieldNotNull] [Export] private PackedScene _characterPrefab;
        public int OwnerPeerId;
        public Node WorldRoot;
        public Node CharacterRoot;
        public StateField<int> Balance;

        public void CatchupState(Array valuesSerialized)
        {
            StateReplicator.LoadValuesCatchup(valuesSerialized);
        }
        
        public void Initialize(SceneTree tree, Node worldRoot, int ownerPeerId,
            PlayerProtocol.InitialState initialState)
        {
            this.InitializeBehavior();
            
            // Initialize state
            var networkVariant = tree.GetNetworkVariant(ownerPeerId);
            PlayerRoot.Name = $"player_{ownerPeerId}";
            WorldRoot = worldRoot;
            OwnerPeerId = ownerPeerId;
            
            // Define replicated values
            Balance = StateReplicator.MakeField(new PrimitiveSerializer<int>());

            // Setup variant
            this.GetGameObject<Node>().ApplyNetworkVariant(networkVariant, typeof(PlayerLogicServer),
                typeof(PlayerLogicClient), typeof(PlayerLogicLocal), typeof(PlayerLogicPuppet));

            // Setup shared
            if (initialState.CharacterState != null) BuildCharacter(networkVariant, initialState.CharacterState);
        }

        public void BuildCharacter(NetworkTypeUtils.ObjectVariant variant, CharacterProtocol.InitialState initialState)
        {
            Debug.Assert(CharacterRoot == null);
            CharacterRoot = _characterPrefab.Instance();
            CharacterRoot.GetBehavior<CharacterLogicShared>().Initialize(this, variant, initialState);
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