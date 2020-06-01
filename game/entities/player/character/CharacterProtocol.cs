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
            PerformMovement,
            Interact
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
        
        public class InteractPacket: Reference
        {
            public static readonly StructSerializer<InteractPacket> Serializer = new StructSerializer<InteractPacket>(
                () => new InteractPacket(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(RotHorizontal)] = new PrimitiveSerializer<float>(),
                    [nameof(RotVertical)] = new PrimitiveSerializer<float>()
                });

            public float RotHorizontal;
            public float RotVertical;
        }
    }
}