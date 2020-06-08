using Godot;
using Overlords.game.entities.player;
using Overlords.helpers;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldServer : Node
    {
        [RequireBehavior] public WorldShared SharedLogic;
        private readonly NodeGroup<string, Node> _groupAutoCatchup = new NodeGroup<string, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();

            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void RegisterAutoCatchup(Node node)
        {
            _groupAutoCatchup.AddToGroup(node.Name, node);
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} joined!");

            var entityContainer = SharedLogic.EntityReplicator;

            // Create and setup player
            var player = SharedLogic.PlayerPrefab.Instance();
            var sharedLogic = player.GetBehavior<PlayerShared>();
            sharedLogic.InitializeLocal(this.GetGameObject<Node>(), peerId, NetworkTypeUtils.ObjectVariant.Server,
                new PlayerProtocol.InitialState
                {
                    Position = new Vector3(
                        (float) GD.RandRange(-10, 10), 10, (float) GD.RandRange(-10, 10))
                });
            entityContainer.AddChild(player);
            RegisterAutoCatchup(player);

            // Replicate player
            entityContainer.SvReplicateInstance(SharedLogic.GetPlayingPeers(), player);
            SharedLogic.Players.AddToGroup(peerId, player);
            entityContainer.SvReplicateInstances(peerId, _groupAutoCatchup.IterateGroupMembers());
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");

            var playerNodeGroup = SharedLogic.Players;
            var player = playerNodeGroup.GetMemberOfGroup<Node>(peerId, this);
            if (player == null) return;

            playerNodeGroup.RemoveFromGroup(player);
            SharedLogic.EntityReplicator.SvDeReplicateInstances(SharedLogic.GetPlayingPeers(), player.AsEnumerable());
            player.Purge();
        }
    }
}