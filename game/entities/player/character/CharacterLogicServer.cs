using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using _PlayerRemoteEventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.character.CharacterProtocol.ServerBound,
    Overlords.game.entities.player.character.CharacterProtocol.ClientBound>;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicServer : Node
    {
        [RequireParent] public KinematicBody Body;
        [RequireBehavior] public CharacterLogicShared LogicShared;
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
                new PrimitiveSerializer<Vector3>(), (sender, selectedPos) =>
                {
                    GD.Print("Interact!!!");
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