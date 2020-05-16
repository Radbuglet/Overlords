using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldControllerServer: Node
    {
        [LinkNodeStatic("../../DynamicEntities")]
        public ListReplicator EntityReplicator;
        
        [LinkNodeStatic("../../RemoteEvents/LoggedIn")]
        public RemoteEvent RemoteLoggedIn;

        [FieldNotNull] [Export] private PackedScene _playerPrefab;
        
        private readonly NodeGroup<int, Node> _players = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_OnPeerConnect));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_OnPeerDisconnect));
            EntityReplicator.CatchupReplicationValidator = (peerId, instance) => true;
        }

        private void _OnPeerConnect(int peerId)
        {
            GD.Print(peerId, " connected!");
            
            // Create player
            var player = _playerPrefab.Instance();
            player.Name = $"player_{peerId}";
            EntityReplicator.AddChild(player);
            
            // Perform puppet replication
            EntityReplicator.PushObjectsRemotely(_players.IterateGroupKeys(), player.AsEnumerable());
            
            // Perform login replication
            _players.AddToGroup(peerId, player);
            GetTree().PerformCatchup(peerId);
            RemoteLoggedIn.FireId(peerId, null);
        }

        private void _OnPeerDisconnect(int peerId)
        {
            GD.Print(peerId, " disconnected!");
            
            var player = _players.GetMemberOfGroup<Node>(peerId, null);
            if (player == null) return;
            _players.RemoveFromGroup(player);
            EntityReplicator.RemoveObjectsRemotely(_players.IterateGroupKeys(), new Array<string>{player.Name});
            player.Purge();
        }
    }
}