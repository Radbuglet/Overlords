﻿using System.Collections.Generic;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world
{
    public static class Protocol
    {
        public class ReplicatedEntity
        {
            public readonly StructSerializer<ReplicatedEntity> Serializer = new StructSerializer<ReplicatedEntity>(
                () => new ReplicatedEntity(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(TypeIndex)] = new PrimitiveSerializer<int>(),
                    [nameof(Constructor)] = new PrimitiveSerializer<object>()
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
            
            public int TypeIndex;
            public object Constructor;
        }
    }
}