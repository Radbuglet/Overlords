using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world.shared
{
    public enum ServerBoundPacketType
    {
        SendMessage
    }
        
    public enum ClientBoundPacketType
    {
        JoinedGame,
        CreateOtherPlayer,
        DeleteOtherPlayer
    }

    public static class Protocol
    {
        public struct PlayerInfoPublic
        {
            public int PeerId;
            public string Name;
            public Vector3 Position;
            public Vector2 Orientation;
            
            public static readonly SimpleStructSerializer<PlayerInfoPublic> Serializer = new SimpleStructSerializer<PlayerInfoPublic>(
                () => new PlayerInfoPublic(), GetFields);

            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield return SerializableStructField.OfPrimitive<int>(nameof(PeerId));
                yield return SerializableStructField.OfPrimitive<string>(nameof(Name));
                yield return SerializableStructField.OfPrimitive<Vector3>(nameof(Position));
                yield return SerializableStructField.OfPrimitive<Vector2>(nameof(Orientation));
            }
        }
        
        public struct CbJoinedGame
        {
            public static readonly SimpleStructSerializer<CbJoinedGame> Serializer = new SimpleStructSerializer<CbJoinedGame>(
                () => new CbJoinedGame(),
                GetFields);
            
            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield break;
            }

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
        
        public struct CbCreateOtherPlayer
        {
            public PlayerInfoPublic PlayerInfo;
            public bool IncludeJoinMessage;
            
            public static readonly SimpleStructSerializer<CbCreateOtherPlayer> Serializer = new SimpleStructSerializer<CbCreateOtherPlayer>(
                () => new CbCreateOtherPlayer(),
                GetFields);

            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield return SerializableStructField.OfStruct(nameof(PlayerInfo), PlayerInfoPublic.Serializer);
                yield return SerializableStructField.OfPrimitive<bool>(nameof(IncludeJoinMessage));
            }

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
    }
}