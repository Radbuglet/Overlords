using System;

namespace Overlords.helpers.network.serialization
{
    public static class CoreSerialization
    {
        public interface ITypelessSerializationPair
        {
            object SerializeTypeless(object data);
            object DeserializeTypeless(object raw);
        }
        
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
    
    public struct TypelessSerializationPair: CoreSerialization.ITypelessSerializationPair
    {
        private readonly Func<object, object> _serializationFunc;
        private readonly Func<object, object> _deserializationFunc;

        public TypelessSerializationPair(Func<object, object> serialize, Func<object, object> deserialize)
        {
            _serializationFunc = serialize;
            _deserializationFunc = deserialize;
        }
        
        public object SerializeTypeless(object data)
        {
            return _serializationFunc(data);
        }

        public object DeserializeTypeless(object raw)
        {
            return _deserializationFunc(raw);
        }

        public static TypelessSerializationPair OfPair<TData>(
            CoreSerialization.Serialize<TData> serialize, CoreSerialization.Deserialize<TData> deserialize)
        {
            return new TypelessSerializationPair(data => serialize((TData) data),
                raw => deserialize(raw));
        }
        
        public static TypelessSerializationPair OfPair<TData, TConfig>(TConfig config,
            CoreSerialization.SerializeConfigured<TData, TConfig> serialize, CoreSerialization.DeserializeConfigured<TData, TConfig> deserialize)
        {
            return new TypelessSerializationPair(data => serialize((TData) data, config),
                raw => deserialize(raw, config));
        }
    }
}