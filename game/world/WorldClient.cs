using Godot;
using Overlords.game.entities.player;
using Overlords.game.entities.player.utils;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.world.WorldProtocol.ClientBound,
    Overlords.game.world.WorldProtocol.ServerBound>;

namespace Overlords.game.world
{
    public class WorldClient : Node
    {
        [Signal]
        public delegate void PuppetPlayerAdded(Node node);
        
        [Signal]
        public delegate void PuppetPlayerRemoved(Node node);
        
        [RequireBehavior] public WorldShared LogicShared;
        private _EventHub _remoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
            
            // Setup remote event
            _remoteEventHub = new _EventHub(LogicShared.RemoteEvent);
            _remoteEventHub.BindHandler(WorldProtocol.ClientBound.Login, WorldProtocol.LoginPacket.Serializer, _Login);
            
            // Setup dynamic entity spawning
            var entityReplicator = LogicShared.EntityReplicator;
            entityReplicator.ClRegisterBuilder((int) WorldProtocol.EntityType.Player, PlayerProtocol.NetworkConstructor.Serializer,
                constructor => SpawnPlayer(constructor, false));
        }

        private bool SpawnPlayer(PlayerProtocol.NetworkConstructor constructor, bool isLocal)
        {
            try
            {
                // TODO: Ensure that only one localPlayer ever exists.
                var player = LogicShared.PlayerPrefab.Instance();
                var sharedLogic = player.GetBehavior<PlayerShared>();
                sharedLogic.InitializeRemote(GetTree(), this.GetGameObject<Node>(), constructor);
                LogicShared.EntityReplicator.AddChild(player);
            }
            catch (DeserializationException e)
            {
                GD.PushWarning($"Failed to create player: {e.Message}");
                return false;
            }

            return true;
        }

        private void _Login(int sender, WorldProtocol.LoginPacket packet)
        {
            var entityReplicator = LogicShared.EntityReplicator;
            if (!SpawnPlayer(packet.LocalPlayer, true))
            {
                // TODO: Crash and burn.
            }
            entityReplicator.ClManuallyCatchupInstances(packet.OtherEntities);
        }
    }
}