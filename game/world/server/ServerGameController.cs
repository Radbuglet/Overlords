using Godot;
using Overlords.game.constants;
using Overlords.game.entity.player;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;
using Overlords.helpers.trackingGroups;

namespace Overlords.game.world.server
{
    public class ServerGameController: Node
    {
        [Export] private readonly NodePath _pathToRemoteEventHub;
        [Export] private readonly NodePath _pathToDynamicEntities;
        [Export][FieldNotNull] private readonly PackedScene _playerPrefab;
        
        [LinkNodePath(nameof(_pathToRemoteEventHub))]
        public RemoteEvent RemoteEvent;
        
        [LinkNodePath(nameof(_pathToDynamicEntities))]
        public EntityContainer DynamicEntities;

        private RemoteEventHub<Protocol.ServerBoundPacket, Protocol.ClientBoundPacket> _remoteEventHub;
        private NodeGroup<int, Node> _playerGroup = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
            _remoteEventHub = new RemoteEventHub<Protocol.ServerBoundPacket, Protocol.ClientBoundPacket>(RemoteEvent);
            AddChild(_remoteEventHub);
            _remoteEventHub.BindHandler(Protocol.ServerBoundPacket.SendMessage,
                (sender, data) => {
                    GD.Print($"{sender} says: {data}");
                });
        }

        private void _PeerConnected(int peerId)
        {
            GD.Print($"{peerId} connected!");
            HandlePlayerJoin(peerId);
        }
        
        private void _PeerDisconnected(int peerId)
        {
            GD.Print($"{peerId} disconnected!");
        }

        private void HandlePlayerJoin(int peerId)
        {
            GD.Print($"{peerId} joined!");
            var player = _playerPrefab.Instance();
            player.GetBehavior<ServerPlayerController>().Configure(peerId);
            _playerGroup.AddToGroup(peerId, player);
        }
    }
}