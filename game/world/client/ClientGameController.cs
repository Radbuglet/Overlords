using Godot;
using Overlords.game.constants;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world.client
{
    public class ClientGameController: Node
    {
        [Export] private readonly NodePath _pathToRemoteEventHub;

        [LinkNodePath(nameof(_pathToRemoteEventHub))]
        public RemoteEvent RemoteEvent;

        private RemoteEventHub<ClientBoundPacketType, ServerBoundPacketType> _remoteEventHub;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.ConnectedToServer, this, nameof(_ConnectionEstablished));
            tree.Connect(SceneTreeSignals.ConnectionFailed, this, nameof(_ConnectionFailed));
            tree.Connect(SceneTreeSignals.ServerDisconnected, this, nameof(_ServerDisconnected));
            _remoteEventHub = new RemoteEventHub<ClientBoundPacketType, ServerBoundPacketType>(RemoteEvent);
            _remoteEventHub.BindHandler(ClientBoundPacketType.JoinedGame, (sender, packet) =>
            {
                try
                {
                    Protocol.CbJoinedGame.Deserialize(packet);
                    GD.Print("Joined game and got a valid packet!");
                }
                catch (CoreSerialization.DeserializationException e)
                {
                    GD.PushWarning("Failed to deserialize joined game packet!");
                }
            });
            _remoteEventHub.BindHandler(ClientBoundPacketType.CreateOtherPlayer, (sender, packet) =>
            {
                GD.Print("Another dude joined!");
            });
            AddChild(_remoteEventHub);
        }

        private void _ConnectionEstablished()
        {
            GD.Print("We connected!");
            _remoteEventHub.Send(ServerBoundPacketType.SendMessage, "Foo bar baz!");
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