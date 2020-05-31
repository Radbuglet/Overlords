using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.player.character
{
    public static class CharacterProtocol
    {
        public enum ClientBound
        {
            PuppetSetPos
        }

        public enum ServerBound
        {
            PerformMovement
        }

        public class InitialState
        {
            public static readonly StructSerializer<InitialState> Serializer = new StructSerializer<InitialState>(
                () => new InitialState(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(Position)] = new PrimitiveSerializer<Vector3>()
                });

            public Vector3 Position;
        }
    }
}