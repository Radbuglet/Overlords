using System.Collections.Generic;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world
{
    public static class Protocol
    {
        public class PlayerConstructor
        {
            public static readonly StructSerializer<PlayerConstructor> Serializer = new StructSerializer<PlayerConstructor>(
                () => new PlayerConstructor(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>()
                });
            
            public int OwnerPeerId;
        }

        public static string GetPlayerName(int peerId)
        {
            return $"player_{peerId}";
        }
    }
}