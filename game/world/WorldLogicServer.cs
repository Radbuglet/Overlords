using Godot;
using Overlords.game.entities.player;
using Overlords.helpers;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicServer : Node
    {
        [RequireBehavior]
        public WorldLogicShared SharedLogic;

        public readonly NodeGroup<string, Node> GroupAutoCatchup = new NodeGroup<string, Node>();
        
        public override void _Ready()
        {
            this.InitializeBehavior();

            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void RegisterAutoCatchup(Node node)
        {
            GroupAutoCatchup.AddToGroup(node.Name, node);
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} joined!");

            var entityContainer = SharedLogic.Entities;
            
            // Create and setup player
            var newPlayer = SharedLogic.PlayerPrefab.Instance();
            newPlayer.GetBehavior<PlayerLogicShared>().SetupPreEntry(GetTree(), GetParent(), peerId, new PlayerProtocol.PlayerInitialState
            {
                Balance = 0,
                CharacterState = new PlayerProtocol.CharacterInitialState
                {
                    Position = new Vector3((float) GD.RandRange(-10, 10), 0, (float) GD.RandRange(-10, 10))
                }
            });
            entityContainer.AddChild(newPlayer);
            RegisterAutoCatchup(newPlayer);
            
            // Replicate player
            entityContainer.SvReplicateInstance(SharedLogic.GetPlayingPeers(), newPlayer);
            SharedLogic.Players.AddToGroup(peerId, newPlayer);
            entityContainer.SvReplicateInstances(peerId, GroupAutoCatchup.IterateGroupMembers());
        }
        
        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");

            var playerNodeGroup = SharedLogic.Players;
            var player = playerNodeGroup.GetMemberOfGroup<Node>(peerId, this);
            if (player == null) return;
            
            playerNodeGroup.RemoveFromGroup(player);
            SharedLogic.Entities.SvDeReplicateInstances(SharedLogic.GetPlayingPeers(), player.AsEnumerable());
            player.Purge();
        }
    }
}