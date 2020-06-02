using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicPuppet : Node
    {
        [RequireBehavior] public CharacterLogicShared LogicShared;
        private RemoteEventHub<CharacterProtocol.ClientBound, CharacterProtocol.ServerBound> _remoteEventHub;

        public override void _Ready()
        {
            this.InitializeBehavior();
            _remoteEventHub = new RemoteEventHub<CharacterProtocol.ClientBound, CharacterProtocol.ServerBound>(LogicShared.RemoteEvent);
            _remoteEventHub.BindHandler(CharacterProtocol.ClientBound.PuppetSetPos, new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    LogicShared.Body.Translation = position;
                });
            AddChild(_remoteEventHub);
        }
    }
}