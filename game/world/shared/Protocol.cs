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
    
    public static class ClientBoundPackets
    {
        public struct CreateOtherPlayer: ISerializableStruct
        {
            public bool ShowJoinMessage;
            public int PeerId;

            public CreateOtherPlayer(bool showJoinMessage, int peerId)
            {
                ShowJoinMessage = showJoinMessage;
                PeerId = peerId;
            }

            public IEnumerable<SerializableStructField> GetSerializedFields()
            {
                yield return new SerializableStructField(
                    nameof(ShowJoinMessage), new PrimitiveSerializer<bool>());
                    
                yield return new SerializableStructField(
                    nameof(PeerId), new PrimitiveSerializer<int>());
            }
        }
    }
}