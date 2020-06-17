using Godot;
using Overlords.game.world.entityCore;
using Overlords.helpers.network.catchup;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world.services
{
    public class JoinHandler: Node
    {
        [Export] private PackedScene _playerPrefab;
        [RequireBehavior] public EntityContainer Entities;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            GetTree().Connect("network_peer_connected", this, nameof(_PeerJoined));
            GetTree().Connect("network_peer_disconnected", this, nameof(_PeerLeft));
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} connected!");
            this.GetGameObject<Node>().CatchupToPeer(peerId);
            var player = _playerPrefab.Instance();
            player.Name = $"player_{peerId}";
            Entities.AddEntity(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");
        }
    }
}