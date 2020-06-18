using Godot;
using Overlords.game.entities.player;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.services;

namespace Overlords.game.world.logic
{
    public class JoinHandler : Node, IParentEnterTrigger
    {
        [Export] private PackedScene _playerPrefab;
        private WorldRoot WorldRoot => GetNode<WorldRoot>("../../");

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
            var player = (PlayerRoot) _playerPrefab.Instance();
            player.Name = $"player_{peerId}";
            player.State.OwnerPeerId.Value = peerId;
            WorldRoot.Entities.AddEntity(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");
        }

        public void _EarlyEditorTrigger(SceneTree tree)
        {
            if (tree.GetNetworkMode() != NetworkMode.Server)
                this.Purge();
        }
    }
}