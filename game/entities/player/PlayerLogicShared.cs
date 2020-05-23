using System;
using System.Diagnostics;
using Godot;
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
        
        public Spatial CharacterRoot;
        public int Balance;

        public void SetupPreEntry(SceneTree tree, Node worldRoot, int peerId, PlayerProtocol.PlayerInitialState state)
        {
            this.InitializeBehavior();
            PlayerRoot.Name = $"player_{peerId}";
            WorldRoot = worldRoot;
            OwnerPeerId = peerId;

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
                        if (OwnerPeerId != tree.GetNetworkUniqueId())
                            gameObject.GetBehavior<PlayerLogicLocal>().Purge();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // Setup shared
            Balance = state.Balance;
            if (state.HasCharacter)
            {
                BuildCharacter(state.CharacterState);
            }
        }

        public void BuildCharacter(PlayerProtocol.CharacterInitialState initialState)
        {
            Debug.Assert(CharacterRoot == null);
            CharacterRoot = (Spatial) _characterPrefab.Instance();
            CharacterRoot.Translation = initialState.Position;
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