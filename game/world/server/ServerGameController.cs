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

        private RemoteEventHub<ServerBoundPacketType, ClientBoundPacketType> _remoteEventHub;
        private readonly NodeGroup<int, Node> _playerGroup = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
            _remoteEventHub = new RemoteEventHub<ServerBoundPacketType, ClientBoundPacketType>(RemoteEvent);
            _remoteEventHub.BindHandler(ServerBoundPacketType.SendMessage,
                (sender, data) => {
                    GD.Print($"{sender} says: {data}");
                });
            AddChild(_remoteEventHub);
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

        private void HandlePlayerJoin(int newPeerId)
        {
            GD.Print($"{newPeerId} joined!");
            
            // Setup player
            var newPlayer = _playerPrefab.Instance();
            var newPlayerPublicState = newPlayer.GetBehavior<PublicPlayerState>();
            newPlayerPublicState.PlayerName = "Some username";
            DynamicEntities.AddEntity(newPeerId.ToString(), newPlayer);

            // Replicate player to other players
            {
                var serializedPacket = new Protocol.CbCreateOtherPlayer
                {
                    IncludeJoinMessage = true,
                    PlayerInfo = newPlayerPublicState.SerializeInfo(newPeerId)
                }.Serialize();
                
                foreach (var oldPeerEntry in _playerGroup.IterateGroupMembersEntries())
                {
                    _remoteEventHub.Send(oldPeerEntry.Key, ClientBoundPacketType.CreateOtherPlayer, serializedPacket);
                }
            }

            // Send catchup data  TODO: Add actual catchup data
            _remoteEventHub.Send(newPeerId, ClientBoundPacketType.JoinedGame, new Protocol.CbJoinedGame().Serialize());
            
            // Register player
            _playerGroup.AddToGroup(newPeerId, newPlayer);
        }
    }
}