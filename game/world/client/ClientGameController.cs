using Godot;
using Overlords.game.constants;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
using Overlords.helpers.network;

namespace Overlords.game.world.client
{
    public class ClientGameController: Node
    {
        [Export] private readonly NodePath _pathToRemoteEventHub;

        [LinkNodePath(nameof(_pathToRemoteEventHub))]
        public RemoteEvent RemoteEvent;

        private RemoteEventHub<Protocol.ClientBoundPacket, Protocol.ServerBoundPacket> _remoteEventHub;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.ConnectedToServer, this, nameof(_ConnectionEstablished));
            tree.Connect(SceneTreeSignals.ConnectionFailed, this, nameof(_ConnectionFailed));
            tree.Connect(SceneTreeSignals.ServerDisconnected, this, nameof(_ServerDisconnected));
            _remoteEventHub = new RemoteEventHub<Protocol.ClientBoundPacket, Protocol.ServerBoundPacket>(RemoteEvent);
            AddChild(_remoteEventHub);
        }

        private void _ConnectionEstablished()
        {
            GD.Print("We connected!");
            _remoteEventHub.Send(Protocol.ServerBoundPacket.SendMessage, "Foo bar baz!");
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