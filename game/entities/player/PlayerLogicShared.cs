﻿using System.Diagnostics;
using Godot;
using Overlords.game.entities.player.client;
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
            PlayerUtils.InitializeControlledVariant(tree, this.GetGameObject<Node>(), peerId, typeof(PlayerLogicServer),
                typeof(PlayerLogicClient), typeof(PlayerLogicLocal), typeof(PlayerLogicPuppet));
            
            // Setup shared
            Balance = state.Balance;
            if (state.CharacterState != null)
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