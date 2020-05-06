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
                () => new PlayerInfoPublic(), new[]
                {
                    SerializableStructField.OfPrimitive<int>(nameof(PeerId)),
                    SerializableStructField.OfPrimitive<string>(nameof(Name)),
                    SerializableStructField.OfPrimitive<Vector3>(nameof(Position)),
                    SerializableStructField.OfPrimitive<Vector2>(nameof(Orientation))
                });
        }
        
        public struct CbJoinedGame
        {
            public static readonly SimpleStructSerializer<CbJoinedGame> Serializer = new SimpleStructSerializer<CbJoinedGame>(
                () => new CbJoinedGame(),
                new SerializableStructField[]{});

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
                new []
                {
                    SerializableStructField.OfStruct(nameof(PlayerInfo), PlayerInfoPublic.Serializer),
                    SerializableStructField.OfPrimitive<bool>(nameof(IncludeJoinMessage))
                });
            
            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
        
        public struct CbDestroyOtherPlayer
        {
            public int PeerId;
            
            public static readonly SimpleStructSerializer<CbDestroyOtherPlayer> Serializer = new SimpleStructSerializer<CbDestroyOtherPlayer>(
                () => new CbDestroyOtherPlayer(),
                new []
                {
                    SerializableStructField.OfPrimitive<int>(nameof(PeerId))
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }

        public static string GetNetworkNameForPlayer(int peerId)
        {
            return $"player_{peerId}";
        }
    }
}