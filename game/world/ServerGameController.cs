using Godot;
using Overlords.game.constants;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;

namespace Overlords.game.world
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

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
            _remoteEventHub = new RemoteEventHub<Protocol.ServerBoundPacket, Protocol.ClientBoundPacket>(RemoteEvent);
            AddChild(_remoteEventHub);
            _remoteEventHub.BindHandler(Protocol.ServerBoundPacket.SendMessage, (sender, data) =>
            {
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

        private static string GetPeerPlayer_EntityId(int peerId)
        {
            return $"player_{peerId}";
        }

        private Node GetPeerPlayer(int peerId)
        {
            return DynamicEntities.GetEntityOrFallback<Node>(GetPeerPlayer_EntityId(peerId), null);
        }

        private void HandlePlayerJoin(int peerId)
        {
            GD.Print($"{peerId} joined!");
            var player = _playerPrefab.Instance();
            DynamicEntities.AddEntity(GetPeerPlayer_EntityId(peerId), player);
        }
    }
}