using System;
using System.Diagnostics;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared: Node
    {
        [FieldNotNull] [Export] private PackedScene _characterPrefab;
        
        [RequireParent] public Spatial PlayerRoot;
        
        [LinkNodeStatic("../ReplicatedState")]
        public StateReplicator StateReplicator;
        
        public int OwnerPeerId;
        public StateField<int> BalanceValue;
        public StateField<bool> HasCharacterValue;

        public Node WorldRoot;
        public Spatial CharacterRoot;

        public void SetupPreEntry(SceneTree tree, Node worldRoot, int peerId)
        {
            this.InitializeBehavior();
            PlayerRoot.Name = $"player_{peerId}";
            WorldRoot = worldRoot;
            OwnerPeerId = peerId;
            BalanceValue = StateReplicator.MakeField(new PrimitiveSerializer<int>());
            HasCharacterValue = StateReplicator.MakeField(new PrimitiveSerializer<bool>());
            
            // Setup variant
            {
                var networkMode = tree.GetNetworkMode();
                var gameObject = this.GetGameObject<Node>();
                switch (networkMode)
                {
                    case NetworkUtils.NetworkMode.None:
                        GD.PushWarning("Player created on non-networked scene tree!");
                        break;
                    case NetworkUtils.NetworkMode.Client:
                        gameObject.GetBehavior<PlayerLogicServer>().Purge();
                        break;
                    case NetworkUtils.NetworkMode.Server:
                        gameObject.GetBehavior<PlayerLogicLocal>().Purge();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void _Ready()
        {
            // TODO: This should be a client only thing.
            SetCharacterBuiltState(HasCharacterValue.Value);
        }

        public void SetCharacterBuiltState(bool state)
        {
            if (state == (CharacterRoot != null)) return;
            if (state) BuildCharacter(); else DestroyCharacter();
        }
        
        public void BuildCharacter()
        {
            Debug.Assert(CharacterRoot == null);
            CharacterRoot = (Spatial) _characterPrefab.Instance();
            CharacterRoot.Translation = new Vector3((float) GD.RandRange(-10, 10), 0, (float) GD.RandRange(-10, 10));
            AddChild(CharacterRoot);
        }

        public void DestroyCharacter()
        {
            Debug.Assert(CharacterRoot != null);
            CharacterRoot.Purge();
        }
    }
}