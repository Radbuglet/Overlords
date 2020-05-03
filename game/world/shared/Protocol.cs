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
        OtherPlayerDisconnected,
    }

    public static class Protocol
    {
        public struct PlayerInfoPublic
        {
            public int PeerId;
            public string Name;
            public Vector3 Position;
            public Vector2 Orientation;

            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield return SerializableStructField.OfPrimitive<int>(nameof(PeerId));
                yield return SerializableStructField.OfPrimitive<string>(nameof(Name));
                yield return SerializableStructField.OfPrimitive<Vector3>(nameof(Position));
                yield return SerializableStructField.OfPrimitive<Vector2>(nameof(Orientation));
            }
            
            public static object Serialize(PlayerInfoPublic data)
            {
                return StructSerialization.Serialize(data, GetFields());
            }

            public static PlayerInfoPublic Deserialize(object raw)
            {
                return StructSerialization.Deserialize(raw, (GetFields(), new PlayerInfoPublic()));
            }
        }
        
        public struct CbCreateOtherPlayer
        {
            public PlayerInfoPublic PlayerInfo;
            public bool IncludeJoinMessage;
            
            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield return SerializableStructField.OfPair(nameof(PlayerInfo), PlayerInfoPublic.Serialize, PlayerInfoPublic.Deserialize);
                yield return SerializableStructField.OfPrimitive<bool>(nameof(IncludeJoinMessage));
            }
            
            public static object Serialize(CbCreateOtherPlayer target)
            {
                return StructSerialization.Serialize(target, GetFields());
            }

            public object Serialize()
            {
                return Serialize(this);
            }
            
            public static CbCreateOtherPlayer Deserialize(object raw)
            {
                return StructSerialization.Deserialize(raw, (GetFields(), new CbCreateOtherPlayer()));
            }
        }
        
        public struct CbJoinedGame
        {
            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield break;
            }
            
            public static object Serialize(CbJoinedGame target)
            {
                return StructSerialization.Serialize(target, GetFields());
            }

            public object Serialize()
            {
                return Serialize(this);
            }
            
            public static CbJoinedGame Deserialize(object raw)
            {
                return StructSerialization.Deserialize(raw, (GetFields(), new CbJoinedGame()));
            }
        }
    }
}