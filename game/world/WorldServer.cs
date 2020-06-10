using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.player;
using Overlords.game.entities.player.common;
using Overlords.helpers;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;
using Overlords.helpers.tree.trackingGroups;

using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.world.WorldProtocol.ServerBound,
    Overlords.game.world.WorldProtocol.ClientBound>;

namespace Overlords.game.world
{
    public class WorldServer : Node
    {
        [RequireBehavior] public WorldShared SharedLogic;
        private readonly NodeGroup<string, Node> _groupAutoCatchup = new NodeGroup<string, Node>();
        private _EventHub _remoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();

            var tree = GetTree();
            _remoteEventHub = new _EventHub(SharedLogic.RemoteEvent);
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void RegisterAutoCatchup(Node node)
        {
            _groupAutoCatchup.AddToGroup(node.Name, node);
        }

        private static (int, object) SerializeEntity(Node instance)
        {
            return instance.GetImplementation<ISerializableEntity>().SerializeConstructor();
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

            // Replicate player to connected peers
            entityContainer.SvReplicateEntities(SharedLogic.GetPlayingPeers(), SerializeEntity(player).AsEnumerable());
            SharedLogic.Players.AddToGroup(peerId, player);

            // Send login info
            _remoteEventHub.FireId(peerId, (WorldProtocol.ClientBound.Login, new WorldProtocol.LoginPacket
            {
                LocalPlayer = player.GetBehavior<PlayerServer>().MakeConstructor(),
                OtherEntities = entityContainer.SvSerializeEntities(
                    _groupAutoCatchup.IterateGroupMembers().Select(SerializeEntity))
            }.Serialize()));
            RegisterAutoCatchup(player);
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