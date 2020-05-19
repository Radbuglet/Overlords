using System.Collections.Generic;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.player
{
    public static class PlayerProtocol
    {
        public class NetworkConstructor
        {
            public static readonly StructSerializer<NetworkConstructor> Serializer = new StructSerializer<NetworkConstructor>(
                () => new NetworkConstructor(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(ReplicatedState)] = new PrimitiveSerializer<Godot.Collections.Dictionary>(),
                    [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>()
                });

            public int OwnerPeerId;
            public Godot.Collections.Dictionary ReplicatedState;
        }
    }
}