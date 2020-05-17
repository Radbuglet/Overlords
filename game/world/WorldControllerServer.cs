using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldControllerServer: Node
    {
        [Export] private NodePath _pathToControllerShared;
        [FieldNotNull] [Export] public PackedScene _playerPrefab;

        [LinkNodePath(nameof(_pathToControllerShared))]
        public WorldControllerShared ControllerShared;
        
        public NodeGroup<int, Node> _playerGroup = new NodeGroup<int, Node>();
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
        }

        private void _PeerConnected(int peerId)
        {
            GD.Print($"{peerId} connected!");
            var entityContainer = ControllerShared.EntityContainer;
            
            // Make player
            var newPlayer = _playerPrefab.Instance();
            newPlayer.Name = Protocol.GetPlayerName(peerId);
            entityContainer.AddChild(newPlayer);
            
            // Replicate it to connected peers
            entityContainer.SvReplicateInstance(_playerGroup.IterateGroupKeys(),
                (newPlayer, ControllerShared.EntityTypePlayer));
            _playerGroup.AddToGroup(peerId, newPlayer);

            // Perform catchup replication on new peer
            IEnumerable<(Node, EntityContainer.RegisteredEntityType)> GetReplicationTargets()
            {
                foreach (var existingPlayer in _playerGroup.IterateGroupMembers())
                {
                    yield return (existingPlayer, ControllerShared.EntityTypePlayer);
                }
            }
            entityContainer.SvReplicateInstances(peerId, GetReplicationTargets());
        }
        
        private void _PeerDisconnected(int peerId)
        {
            GD.Print($"{peerId} disconnected!");
            var player = _playerGroup.GetMemberOfGroup<Node>(peerId, null);
            if (player != null)
            {
                GD.Print("Player removed!");
                _playerGroup.RemoveFromGroup(peerId);
                ControllerShared.EntityContainer.SvDeReplicateInstances(_playerGroup.IterateGroupKeys(), player.AsEnumerable());
                player.Purge();
            }
        }
    }
}