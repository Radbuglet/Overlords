using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.player.common
{
    public static class PlayerProtocol
    {
        // Construction
        public class NetworkConstructor
        {
            public static readonly StructSerializer<NetworkConstructor> Serializer = new StructSerializer<NetworkConstructor>(
                () => new NetworkConstructor(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>(),
                    [nameof(InitialState)] = InitialState.Serializer
                });

            public int OwnerPeerId;
            public InitialState InitialState;

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
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
        
        // Remotes
        public enum ClientBound
        {
            PuppetSetPos
        }

        public enum ServerBound
        {
            PerformMovement,
            Interact
        }
        
        public class InteractPacket: Reference
        {
            public static readonly StructSerializer<InteractPacket> Serializer = new StructSerializer<InteractPacket>(
                () => new InteractPacket(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(TargetId)] = new PrimitiveSerializer<string>(),
                    [nameof(InteractPoint)] = new PrimitiveSerializer<Vector3>()
                });

            public string TargetId;
            public Vector3 InteractPoint;

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
    }
}