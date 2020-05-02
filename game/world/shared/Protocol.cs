using System.Collections.Generic;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world.shared
{
    public static class Protocol
    {
        public enum ServerBoundPacketType
        {
            SendMessage
        }
        
        public enum ClientBoundPacketType
        {
            LoggedIn,
            CreateOtherPlayer,
            OtherPlayerDisconnected,
        }
    }

    public static class ClientBound
    {
        public class CreateOtherPlayer
        {
            public int PeerId;
            public bool IncludeJoinMessage;
            
            private static IEnumerable<SerializableStructField> GetFields()
            {
                yield return SerializableStructField.OfPrimitive<int>(nameof(PeerId));
                yield return SerializableStructField.OfPrimitive<bool>(nameof(IncludeJoinMessage));
            }
            
            public static object Serialize(CreateOtherPlayer target)
            {
                return StructSerialization.Serialize(target, GetFields());
            }
            
            public static CreateOtherPlayer Deserialize(object raw)
            {
                return StructSerialization.Deserialize(raw, (GetFields(), new CreateOtherPlayer()));
            }
        }
    }
}