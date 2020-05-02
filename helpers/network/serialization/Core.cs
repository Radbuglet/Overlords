using System;

namespace Overlords.helpers.network.serialization
{
    public static class SerializationCore
    {
        public class DeserializationException: Exception
        {
            public DeserializationException(string reason) : base(reason)
            {
                
            }

            public DeserializationException(): base("Failed to deserialize because of an unspecified reason.")
            {
                
            }
        }

        public interface ITypelessHandler
        {
            object SerializeTypeless(object raw);
            object DeserializeTypeless(object raw);
        }
        
        public interface IUpCastableHandler<T>
        {
            object Serialize(T structData);
            T Deserialize(object raw);
        }
    }
}