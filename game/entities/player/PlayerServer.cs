using Godot;
using Overlords.game.entities.common;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.PlayerProtocol.ServerBound,
    Overlords.game.entities.player.PlayerProtocol.ClientBound>;

namespace Overlords.game.entities.player
{
    public class PlayerServer: Spatial
    {
        private KinematicBody Body => LogicShared.GetBody();
        [RequireBehavior] public PlayerShared LogicShared;
        [LinkNodeStatic("../FpsCamera/RayCast")] public Spatial RayCastOrigin;
        public _EventHub RemoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
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
            
            // TODO: Re-implement server logic
            BindOwnerHandler(PlayerProtocol.ServerBound.PerformMovement,
                new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    Body.Translation = position;
                    RemoteEventHub.FireId(LogicShared.GetWorldShared().GetPlayingPeers(sender),
                        (PlayerProtocol.ClientBound.PuppetSetPos, (object) position));
                });

            /*BindOwnerHandler(PlayerProtocol.ServerBound.PerformMovement,
                new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    Body.Translation = position;
                    RemoteEventHub.FireId(LogicShared.World.GetPlayingPeers(sender),
                        (CharacterProtocol.ClientBound.PuppetSetPos, (object) position));
                });
            
            BindOwnerHandler(CharacterProtocol.ServerBound.Interact,
                CharacterProtocol.InteractPacket.Serializer, (sender, packet) =>
                {
                    var interactionTarget = LogicShared.WorldShared.Targets.GetMemberOfGroup<Spatial>(packet.TargetId, null);
                    if (interactionTarget == null)
                    {
                        GD.PushWarning("Ignoring interact request: unknown interaction target!");
                        return;
                    }

                    var rayFrom = RayCastOrigin.GetGlobalPosition();
                    var rayTo = interactionTarget.GetGlobalPosition() + packet.InteractPoint;
                    if (rayFrom.DistanceTo(rayTo) > CharacterLogicShared.InteractDistance)
                    {
                        GD.PushWarning("Ignoring interact request: target out of range!");
                        return;
                    }
                    
                    var intersection = GetWorld().DirectSpaceState.IntersectRay(
                        rayFrom, rayTo + (rayTo - rayFrom).Normalized() * 0.2F, new Array{ Body });
                    if (!intersection.Contains("collider") || intersection["collider"] != interactionTarget)
                    {
                        GD.PushWarning("Ignoring interact request: mismatched collider!");
                        return;
                    }
                    interactionTarget.FireEntitySignal(nameof(GameSignals.OnEntityInteracted), this.GetGameObject<Node>());
                });*/
        }

        public PlayerProtocol.NetworkConstructor MakeConstructor(int target)
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
    }
}