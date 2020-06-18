using Godot;
using Overlords.game.entities.player;
using Overlords.game.world.entityCore;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.services;

namespace Overlords.game.world.logic
{
    public class JoinHandler : Node, IParentEnterTrigger
    {
        // TODO: Use tracking groups instead of searching by name because the later is inefficient.
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
            WorldRoot.CatchupToPeer(peerId);

            var entityContainer = WorldRoot.Entities;
            var player = (PlayerRoot) _playerPrefab.Instance();
            player.Name = EntityTypes.GetPlayerName(peerId);
            entityContainer.AddChild(player);
            
            player.State.DisplayName.Value = "radbuglet";
            player.State.OwnerPeerId.Value = peerId;
            player.State.Balance.Value = 0;
            player.State.InitialPosition.Value = new Vector3(
                (float) GD.RandRange(-10, 10), 10,
                (float) GD.RandRange(-10, 10));
            player.SharedLogic.OnSetupComplete();
            
            entityContainer.ReplicateEntity(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");
            var entityContainer = WorldRoot.Entities;
            var player = entityContainer.GetNodeOrNull<PlayerRoot>(EntityTypes.GetPlayerName(peerId));
            if (player == null) return;
            player.Purge();
            entityContainer.DeReplicateEntity(player);
        }

        public void _EarlyEditorTrigger(SceneTree tree)
        {
            if (tree.GetNetworkMode() != NetworkMode.Server)
                this.Purge();
        }
    }
}