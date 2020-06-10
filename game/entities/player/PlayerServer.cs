using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.common;
using Overlords.game.entities.player.common;
using Overlords.game.world;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;
using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.common.PlayerProtocol.ServerBound,
    Overlords.game.entities.player.common.PlayerProtocol.ClientBound>;

namespace Overlords.game.entities.player
{
    public class PlayerServer: Spatial, ISerializableEntity
    {
        private KinematicBody Body => LogicShared.GetBody();
        [RequireBehavior] public PlayerShared LogicShared;
        public _EventHub RemoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
            this.DeclareImplementation(new []{ typeof(ISerializableEntity) });
            RemoteEventHub = new _EventHub(LogicShared.RemoteEvent);
            void BindOwnerHandler<T>(PlayerProtocol.ServerBound type, ISerializer<T> serializer, _EventHub.PacketHandler<T> handler)
            {
               RemoteEventHub.BindHandler(type, serializer, (sender, packet) =>
               {
                   if (sender != LogicShared.OwnerPeerId)
                   {
                       GD.PushWarning("Non-owning player tried to send an owner-only packet!");
                       return;
                   }
                   handler(sender, packet);
               });
            }
            
            BindOwnerHandler(PlayerProtocol.ServerBound.PerformMovement,
                new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    Body.Translation = position;
                    RemoteEventHub.FireId(LogicShared.GetWorldShared().GetPlayingPeers(sender),
                        (PlayerProtocol.ClientBound.PuppetSetPos, (object) position));
                });

            BindOwnerHandler(PlayerProtocol.ServerBound.Interact,
                PlayerProtocol.InteractPacket.Serializer, (sender, packet) =>
                {
                    var interactionTarget = LogicShared.GetWorldShared().InteractionTargets.GetMemberOfGroup<Spatial>(packet.TargetId, null);
                    if (interactionTarget == null)
                    {
                        GD.PushWarning("Ignoring interact request: unknown interaction target!");
                        return;
                    }

                    if (!this.ValidateInteraction(interactionTarget, packet.InteractPoint,
                        InteractionUtils.MaxInteractionDistance))
                    {
                        GD.PushWarning("Ignoring interact request: interaction isn't valid!");
                        return;
                    }
                    
                    interactionTarget.FireEntitySignal(nameof(GameSignals.OnEntityInteracted), LogicShared.GetBody());
                });
        }

        public PlayerProtocol.NetworkConstructor MakeConstructor()
        {
            return new PlayerProtocol.NetworkConstructor
            {
                OwnerPeerId = LogicShared.OwnerPeerId,
                InitialState = new PlayerProtocol.InitialState
                {
                    Position = LogicShared.Position
                },
                ReplicatedValues = LogicShared.StateReplicator.SerializeValuesCatchup()
            };
        }

        public (int typeId, object constructor) SerializeConstructor()
        {
            return ((int) WorldProtocol.EntityType.Player, MakeConstructor().Serialize());
        }
    }
}