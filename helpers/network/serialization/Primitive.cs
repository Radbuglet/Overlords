using System;
using Godot;
using static Overlords.helpers.network.serialization.CoreSerialization;

namespace Overlords.helpers.network.serialization
{
    public static class PrimitiveSerialization
    {
        public static object Serialize<TData>(TData data)
        {
            return data;
        }
        
        public static TData Deserialize<TData>(object raw)
        {
            return raw is TData data ? data :
                throw new DeserializationException("Primitive was of invalid type.");
        }

        public static object SerializeValidated<TData>(TData data, Func<TData, bool> validator)
        {
            if (!validator(data))
                GD.PushWarning("Serialized primitive that didn't conform to own validation standards. Produced packet may be invalid.");
            return data;
        }

        public static TData DeserializeValidated<TData>(object raw, Func<TData, bool> validator)
        {
            var data = Deserialize<TData>(raw);
            return validator(data) ? data :
                throw new DeserializationException("Primitive does not conform to validation standards.");
        }
    }
}