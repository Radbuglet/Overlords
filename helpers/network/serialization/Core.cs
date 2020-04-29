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

        public interface IUnsafeHandler<out T>
        {
            object SerializeTypeless(object raw);
            T Deserialize(object raw);
        }
        
        public interface IHandler<T>: IUnsafeHandler<T>
        {
            object Serialize(T data);
        }
    }
}