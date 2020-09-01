using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Overlords.game.player;
using Overlords.game.props;
using Overlords.game.shared;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.replication;
using Overlords.helpers.tree;

namespace Overlords.game
{
	public class GameRoot : Spatial, ICullsNetwork
	{
		[FieldNotNull] [Export] private PackedScene _playerPrefab;
		[FieldNotNull] [Export] private NodePath _pathToSpawn;
		
		[LinkNodeStatic("Entities")] public ListReplicator Entities;
		[LinkNodeStatic("State")] public GameState State;
		[LinkNodeEditor(nameof(_pathToSpawn))] public Position3D OverlordSpawnPoint;
		
		public readonly NodeDictionary<int, PlayerRoot> OnlinePlayers = new NodeDictionary<int, PlayerRoot>();
		public readonly NodeDictionary<string, Node> InteractTargets = new NodeDictionary<string, Node>();
		
		// Event handlers
		public override void _Ready()
		{
			this.Initialize();
			
			// Register interact targets
			foreach (var target in GetTree().GetNodesInGroup(Constants.RegisterInteractable).Cast<Node>())
			{
				Debug.Assert(target is IProp);
				InteractTargets.Add(target.Name, target);
			}
			
			if (this.GetNetworkMode() == NetworkMode.Server)
			{
				// Connect signals
				GetTree().Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerConnected));
				GetTree().Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerDisconnected));
			}
		}

		public void _PeerConnected(int peerId)
		{
			GD.Print(peerId, " connected!");

			// Create player
			var player = (PlayerRoot) _playerPrefab.Instance();
			player.Name = $"player_{peerId}";
			Entities.AddChild(player);
			player.State.OwnerPeerId = peerId;
			player.State.DisplayName = $"user_{peerId}";
			player.Shared.OnSetupComplete();
			
			// Replicate to already existent peers
			Entities.ReplicateEntity(player);  // The peer doesn't yet have visibility over the scene and thus won't get the entity replicated by this method.

			// Replicate to the new peer
			OnlinePlayers.Add(peerId, player);  // The replicated scene is now visible to that peer
			RpcId(peerId, nameof(_LoggedIn), this.GenerateCatchupInfo(peerId));  // And thus, when catching up the scene, the peer will receive everything.
		}
		
		public void _PeerDisconnected(int peerId)
		{
			GD.Print(peerId, " disconnected!");
			if (!OnlinePlayers.TryGetValue(peerId, out var player))
				return;

			OnlinePlayers.Remove(peerId);
			Entities.DeReplicateEntity(player);
			player.Purge();
		}

		[Puppet]
		private void _LoggedIn(Dictionary catchupInfo)
		{
			GD.Print("Received remote catchup info.");
			var error = GetTree().ApplyCatchupInfo(catchupInfo);
			if (error != null)
			{
				GD.PushError(error.GetMessage());
				GetTree().Quit();
				return;
			}
			GD.Print("Logged in!");
		}
		
		// Catchup
		public bool IsLocallyVisibleTo(int peerId)
		{
			return OnlinePlayers.HasKey(peerId);
		}
	}
}
