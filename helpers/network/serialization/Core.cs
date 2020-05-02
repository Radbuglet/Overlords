using System;

namespace Overlords.helpers.network.serialization
{
    public static class CoreSerialization
    {
        public class DeserializationException : Exception
        {
            public DeserializationException() : base("Deserialization failed for unspecified reason") {}

            public DeserializationException(string reason) : base(reason) {}
        }

        public delegate object Serialize<in TData>(TData data);
        public delegate object SerializeConfigured<in TData, in TConfig>(TData data, TConfig config);
        public delegate TData Deserialize<out TData>(object raw);
        public delegate TData DeserializeConfigured<out TData, in TConfig>(object raw, TConfig config);
    }
}