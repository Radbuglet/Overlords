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
            
            public static readonly StructSerializer<PlayerInfoPublic> Serializer = new StructSerializer<PlayerInfoPublic>(
                () => new PlayerInfoPublic(), new System.Collections.Generic.Dictionary<string, ISerializerRaw>
                {
                    [nameof(PeerId)] = new PrimitiveSerializer<int>(),
                    [nameof(Name)] = new PrimitiveSerializer<string>(),
                    [nameof(Position)] = new PrimitiveSerializer<Vector3>(),
                    [nameof(Orientation)] = new PrimitiveSerializer<Vector2>()
                });
        }
        
        public class CbJoinedGame
        {
            public Array<PlayerInfoPublic> OtherPlayers;
            
            public static readonly StructSerializer<CbJoinedGame> Serializer = new StructSerializer<CbJoinedGame>(
                () => new CbJoinedGame(),
                new System.Collections.Generic.Dictionary<string, ISerializerRaw>
                {
                    [nameof(OtherPlayers)] = new ListSerializer<PlayerInfoPublic>(PlayerInfoPublic.Serializer)
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
        
        public class CbCreateOtherPlayer
        {
            public PlayerInfoPublic PlayerInfo;
            public bool IncludeJoinMessage;
            
            public static readonly StructSerializer<CbCreateOtherPlayer> Serializer = new StructSerializer<CbCreateOtherPlayer>(
                () => new CbCreateOtherPlayer(),
                new System.Collections.Generic.Dictionary<string, ISerializerRaw>
                {
                    [nameof(PlayerInfo)] = PlayerInfoPublic.Serializer,
                    [nameof(IncludeJoinMessage)] = new PrimitiveSerializer<bool>()
                });
            
            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
        
        public class CbDestroyOtherPlayer
        {
            public int PeerId;
            
            public static readonly StructSerializer<CbDestroyOtherPlayer> Serializer = new StructSerializer<CbDestroyOtherPlayer>(
                () => new CbDestroyOtherPlayer(),
                new System.Collections.Generic.Dictionary<string, ISerializerRaw>
                {
                    [nameof(PeerId)] = new PrimitiveSerializer<int>()
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