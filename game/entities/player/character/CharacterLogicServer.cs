using Godot;
using Godot.Collections;
using Overlords.game.constants;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;
using _PlayerRemoteEventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.character.CharacterProtocol.ServerBound,
    Overlords.game.entities.player.character.CharacterProtocol.ClientBound>;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicServer : Spatial
    {
        [RequireParent] public KinematicBody Body;
        [RequireBehavior] public CharacterLogicShared LogicShared;
        [LinkNodeStatic("../FpsCamera/RayCast")]
        public Spatial RayCastOrigin;
        public _PlayerRemoteEventHub RemoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
            RemoteEventHub = new _PlayerRemoteEventHub(LogicShared.RemoteEvent);
            void BindOwnerHandler<T>(CharacterProtocol.ServerBound type, ISerializer<T> serializer, _PlayerRemoteEventHub.PacketHandler<T> handler)
            {
               RemoteEventHub.BindHandler(type, serializer, (sender, packet) =>
               {
                   if (sender != LogicShared.PlayerShared.OwnerPeerId)
                   {
                       GD.PushWarning("Non-owning player tried to send an owner-only packet!");
                       return;
                   }
                   handler(sender, packet);
               }); 
            }

            BindOwnerHandler(CharacterProtocol.ServerBound.PerformMovement,
                new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    Body.Translation = position;
                    RemoteEventHub.FireId(LogicShared.GetWorldShared().GetPlayingPeers(sender),
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
                });
        }

        public CharacterProtocol.InitialState MakeConstructor(int target)
        {
            return new CharacterProtocol.InitialState
            {
                Position = Body.Translation
            };
        }
    }
}