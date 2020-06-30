using Godot;
using Godot.Collections;
using Overlords.game.entities.player;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.trackingGroup;

namespace Overlords.game.world.logic
{
    public class LoginHandler : Node
    {
        [Export] private PackedScene _playerPrefab;
        private readonly NodeGroup<int, PlayerRoot> _players = new NodeGroup<int, PlayerRoot>();
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
            RpcId(peerId, nameof(_LoggedIn), WorldRoot.GenerateCatchupInfo(peerId));

            // Create player and add it to the tree
            var entityContainer = WorldRoot.Entities;
            var player = (PlayerRoot) _playerPrefab.Instance();
            player.Name = $"player_{peerId}";
            entityContainer.AddChild(player);
            
            // Setup player state
            player.State.DisplayName.Value = "radbuglet";
            player.State.OwnerPeerId.Value = peerId;
            player.State.Balance.Value = 0;
            player.SetGlobalPosition(new Vector3((float) GD.RandRange(-50, 50), 0, (float) GD.RandRange(-50, 50)));
            
            // Register player and replicate
            player.SharedLogic.OnSetupComplete();
            _players.AddToGroup(peerId, player);
            entityContainer.ReplicateEntity(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");
            var entityContainer = WorldRoot.Entities;
            var player = _players.GetMemberOfGroup<PlayerRoot>(peerId, null);
            if (player == null) return;
            player.Purge();
            entityContainer.DeReplicateEntity(player);
        }

        private void _LoggedIn(Dictionary catchupData)
        {
            ApplyCatchupInfo(catchupData);
        }

        public void ApplyCatchupInfo(Dictionary catchupData)
        {
            var error = GetTree().ApplyCatchupInfo(catchupData);
            if (error != null)
                ClientQuit(error.ToMessage());
        }

        public void ClientQuit(string message)
        {
            GD.PushWarning($"Quit back to main menu with message: {message}");
            GetTree().ChangeScene("res://menu/MainMenu.tscn");
        }
    }
}