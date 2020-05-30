using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicServer: Node
    {
        [RequireParent] public KinematicBody Body;
        [RequireBehavior] public CharacterLogicShared LogicShared;
        public RemoteEventHub<CharacterProtocol.ServerBound, CharacterProtocol.ClientBound> RemoteEventHub;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            RemoteEventHub = new RemoteEventHub<CharacterProtocol.ServerBound, CharacterProtocol.ClientBound>(LogicShared.RemoteEvent);
            RemoteEventHub.BindHandler(CharacterProtocol.ServerBound.PerformMovement, new PrimitiveSerializer<Vector3>(),
                (sender, position) =>
                {
                    // TODO: Better protocol; ensure proper owner
                    Body.Translation = position;
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