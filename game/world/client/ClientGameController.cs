using Godot;
using Overlords.game.constants;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;
using Overlords.helpers.trackingGroups;

namespace Overlords.game.world.client
{
    public class ClientGameController: Node
    {
        [Export] private readonly NodePath _pathToRemoteEventHub;
        [Export] private readonly NodePath _pathToDynamicEntities;
        [FieldNotNull] [Export] private readonly PackedScene _playerPrefab;

        [LinkNodePath(nameof(_pathToRemoteEventHub))]
        public RemoteEvent RemoteEvent;

        [LinkNodePath(nameof(_pathToDynamicEntities))]
        public EntityContainer DynamicEntities;

        private RemoteEventHub<ClientBoundPacketType, ServerBoundPacketType> _remoteEventHub;
        private readonly NodeGroup<int, Node> _players = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.ConnectedToServer, this, nameof(_ConnectionEstablished));
            tree.Connect(SceneTreeSignals.ConnectionFailed, this, nameof(_ConnectionFailed));
            tree.Connect(SceneTreeSignals.ServerDisconnected, this, nameof(_ServerDisconnected));
            _remoteEventHub = new RemoteEventHub<ClientBoundPacketType, ServerBoundPacketType>(RemoteEvent);
            _remoteEventHub.BindHandler(ClientBoundPacketType.JoinedGame, Protocol.CbJoinedGame.Serializer,
                (sender, packet) =>
            {
                GD.Print($"Caught up and received {packet.OtherPlayers.Count} player(s)!");
            });
            
            _remoteEventHub.BindHandler(ClientBoundPacketType.CreateOtherPlayer, Protocol.CbCreateOtherPlayer.Serializer,
                (sender, packet) =>
                {
                    var newPlayerPeerId = packet.PlayerInfo.PeerId;
                    var newPlayerInstance = _playerPrefab.Instance();
                    _players.AddToGroup(newPlayerPeerId, newPlayerInstance);
                    DynamicEntities.AddEntity(Protocol.GetNetworkNameForPlayer(newPlayerPeerId), newPlayerInstance);
                    GD.Print($"Puppet player joined with peer id {newPlayerPeerId}");
                });
            
            _remoteEventHub.BindHandler(ClientBoundPacketType.DeleteOtherPlayer, Protocol.CbDestroyOtherPlayer.Serializer,
                (sender, packet) =>
                {
                    var disconnectedPlayer = _players.GetMemberOfGroup<Node>(packet.PeerId, null);
                    if (disconnectedPlayer == null)
                    {
                        GD.PushWarning($"Failed to remove puppet player. Player with PeerId {packet.PeerId} doesn't exist.");
                        return;
                    }
                    
                    disconnectedPlayer.QueueFree();
                    GD.Print($"Puppet player left with peer id {packet.PeerId}");
                });
            AddChild(_remoteEventHub);
        }

        private void _ConnectionEstablished()
        {
            GD.Print("We connected!");
            _remoteEventHub.Fire(null, true, ServerBoundPacketType.SendMessage, "Foo bar baz!");
        }
        
        private void _ConnectionFailed()
        {
            GD.Print("We failed to connect!");
        }
        
        private void _ServerDisconnected()
        {
            GD.Print("We disconnected!");
        }
    }
}