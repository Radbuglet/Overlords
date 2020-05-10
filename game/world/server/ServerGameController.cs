using Godot;
using Godot.Collections;
using Overlords.game.constants;
using Overlords.game.entity.player;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
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

        private RemoteEventHub<ServerBoundPacketType, ClientBoundPacketType> _remoteEventHub;
        private readonly NodeGroup<int, Node> _playerGroup = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
            _remoteEventHub = new RemoteEventHub<ServerBoundPacketType, ClientBoundPacketType>(RemoteEvent);
            _remoteEventHub.BindHandler(ServerBoundPacketType.SendMessage, new PrimitiveSerializer<string>(),
                (sender, data) => {
                    GD.Print($"{sender} says: {data}");
                });
            AddChild(_remoteEventHub);
        }

        private void BroadcastToPlayers(ClientBoundPacketType type, object data)
        {
            foreach (var peerEntry in _playerGroup.IterateGroupMembersEntries())
            {
                _remoteEventHub.Fire(peerEntry.Key, true, type, data);
            }
        }

        private void _PeerConnected(int peerId)
        {
            GD.Print($"{peerId} connected!");
            HandlePlayerJoin(peerId);
        }
        
        private void _PeerDisconnected(int peerId)
        {
            GD.Print($"{peerId} disconnected!");
            var disconnectedPlayer = _playerGroup.GetMemberOfGroup<Node>(peerId, null);
            if (disconnectedPlayer == null) return;
            HandlePlayerLeave(peerId, disconnectedPlayer);
        }

        private void HandlePlayerJoin(int newPeerId)
        {
            GD.Print($"{newPeerId} joined!");
            
            // Setup player
            var newPlayer = _playerPrefab.Instance();
            var newPlayerPublicState = newPlayer.GetBehavior<PublicPlayerState>();
            newPlayerPublicState.PlayerName = "Some username";
            DynamicEntities.AddEntity(Protocol.GetNetworkNameForPlayer(newPeerId), newPlayer);

            // Replicate player to other players
            BroadcastToPlayers(ClientBoundPacketType.CreateOtherPlayer, new Protocol.CbCreateOtherPlayer
            {
                IncludeJoinMessage = true,
                PlayerInfo = newPlayerPublicState.SerializeInfo(newPeerId)
            }.Serialize());

            // Send catchup data
            _remoteEventHub.Fire(newPeerId, true, ClientBoundPacketType.JoinedGame, new Protocol.CbJoinedGame
            {
                OtherPlayers = new Array<Protocol.PlayerInfoPublic>()
            }.Serialize());
            
            // Register player
            _playerGroup.AddToGroup(newPeerId, newPlayer);
        }

        private void HandlePlayerLeave(int leftPeerId, Node playerRoot)
        {
            // Handle leave logic
            _playerGroup.RemoveFromGroup(leftPeerId);  // We need to unregister the player from the group immediately.
            playerRoot.QueueFree();

            // Replicate disconnect to remaining peers
            BroadcastToPlayers(ClientBoundPacketType.DeleteOtherPlayer, new Protocol.CbDestroyOtherPlayer
            {
                PeerId = leftPeerId
            }.Serialize());
        }
    }
}