using Godot;
using Godot.Collections;
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
                    PrimitiveSerialization.MakePair<int>().ForField(nameof(PeerId)),
                    PrimitiveSerialization.MakePair<string>().ForField(nameof(Name)),
                    PrimitiveSerialization.MakePair<Vector3>().ForField(nameof(Position)),
                    PrimitiveSerialization.MakePair<Vector2>().ForField(nameof(Orientation))
                });
        }
        
        public struct CbJoinedGame
        {
            public Array<PlayerInfoPublic> OtherPlayers;
            
            public static readonly SimpleStructSerializer<CbJoinedGame> Serializer = new SimpleStructSerializer<CbJoinedGame>(
                () => new CbJoinedGame(),
                new[]
                {
                    ListSerialization.MakePair<PlayerInfoPublic>(PlayerInfoPublic.Serializer).ForField(nameof(OtherPlayers))
                });

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
                    PlayerInfoPublic.Serializer.ForField(nameof(PlayerInfo)),
                    PrimitiveSerialization.MakePair<bool>().ForField(nameof(IncludeJoinMessage))
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
                    PrimitiveSerialization.MakePair<int>().ForField(nameof(PeerId))
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