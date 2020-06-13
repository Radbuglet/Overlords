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
        [RequireBehavior] public WorldShared LogicShared;
        private readonly NodeGroup<string, Node> _groupAutoCatchup = new NodeGroup<string, Node>();
        private _EventHub _remoteEventHub;

        public override void _Ready()
        {
            // Setup behavior
            this.InitializeBehavior();

            // Connect events
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
            
            // Setup event hub
            _remoteEventHub = new _EventHub(LogicShared.RemoteEvent);
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

            var entityContainer = LogicShared.EntityReplicator;

            // Create and setup player
            var player = LogicShared.PlayerPrefab.Instance();
            var sharedLogic = player.GetBehavior<PlayerShared>();
            sharedLogic.InitializeLocal(this.GetGameObject<Node>(), peerId, NetworkTypeUtils.ObjectVariant.Server,
                new PlayerProtocol.InitialState
                {
                    Position = new Vector3(
                        (float) GD.RandRange(-10, 10), 10, (float) GD.RandRange(-10, 10))
                });
            entityContainer.AddChild(player);

            // Replicate player to connected peers
            entityContainer.SvReplicateEntities(LogicShared.GetPlayingPeers(), SerializeEntity(player).AsEnumerable());
            LogicShared.Players.AddToGroup(peerId, player);

            // Send login info
            _remoteEventHub.FireId(peerId, (WorldProtocol.ClientBound.Login, new WorldProtocol.LoginPacket
            {
                OverlordId = LogicShared.ActiveOverlordPeer,
                LocalPlayer = player.GetBehavior<PlayerServer>().MakeConstructor(),
                OtherEntities = entityContainer.SvSerializeEntities(
                    _groupAutoCatchup.IterateGroupMembers().Select(SerializeEntity))
            }.Serialize()));
            RegisterAutoCatchup(player);
        }

        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");

            var playerNodeGroup = LogicShared.Players;
            var player = playerNodeGroup.GetMemberOfGroup<Node>(peerId, this);
            if (player == null) return;

            playerNodeGroup.RemoveFromGroup(player);
            LogicShared.EntityReplicator.SvDeReplicateInstances(LogicShared.GetPlayingPeers(), player.AsEnumerable());
            player.Purge();
        }
    }
}