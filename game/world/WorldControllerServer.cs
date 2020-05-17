using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerServer: Node
    {
        [Export] private NodePath _pathToControllerShared;

        [LinkNodePath(nameof(_pathToControllerShared))]
        public WorldControllerShared ControllerShared;
        
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
        }
        
        private void _PeerDisconnected(int peerId)
        {
            GD.Print($"{peerId} disconnected!");
        }
    }
}