﻿using Godot;
using Overlords.game.entities.player;
using Overlords.game.entities.player.character;
using Overlords.helpers;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicServer : Node
    {
        [RequireBehavior] public WorldLogicShared SharedLogic;
        private readonly NodeGroup<string, Node> _groupAutoCatchup = new NodeGroup<string, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();

            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void RegisterAutoCatchup(Node node)
        {
            _groupAutoCatchup.AddToGroup(node.Name, node);
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} joined!");

            var entityContainer = SharedLogic.EntityReplicator;

            // Create and setup player
            var newPlayer = SharedLogic.PlayerPrefab.Instance();
            var sharedLogic = newPlayer.GetBehavior<PlayerLogicShared>();
            sharedLogic.Initialize(GetTree(), this.GetGameObject<Node>(), peerId,
                new PlayerProtocol.InitialState
                {
                    CharacterState = new CharacterProtocol.InitialState
                    {
                        Position = new Vector3((float) GD.RandRange(-10, 10), 0, (float) GD.RandRange(-10, 10))
                    }
                });
            sharedLogic.Balance.Value = (int) GD.RandRange(0, 100);
            entityContainer.AddChild(newPlayer);
            RegisterAutoCatchup(newPlayer);

            // Replicate player
            entityContainer.SvReplicateInstance(SharedLogic.GetPlayingPeers(), newPlayer);
            SharedLogic.Players.AddToGroup(peerId, newPlayer);
            entityContainer.SvReplicateInstances(peerId, _groupAutoCatchup.IterateGroupMembers());
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");

            var playerNodeGroup = SharedLogic.Players;
            var player = playerNodeGroup.GetMemberOfGroup<Node>(peerId, this);
            if (player == null) return;

            playerNodeGroup.RemoveFromGroup(player);
            SharedLogic.EntityReplicator.SvDeReplicateInstances(SharedLogic.GetPlayingPeers(), player.AsEnumerable());
            player.Purge();
        }
    }
}