using Godot;
using Overlords.game.world.entityCore;
using Overlords.helpers.csharp;
using Overlords.helpers.network.catchup;
using Overlords.helpers.tree.initialization;

namespace Overlords.game.world.services
{
    public class JoinHandler: Node
    {
        [Export] private PackedScene _playerPrefab;
        [LinkNodeStatic("../Entities")] public EntityContainer EntityContainer;
        
        public override void _Ready()
        {
            this.Initialize();
            GetTree().Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            GetTree().Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} connected!");
            GetParent().CatchupToPeer(peerId);
            var player = _playerPrefab.Instance();
            player.Name = $"player_{peerId}";
            EntityContainer.AddEntity(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");
        }
    }
}