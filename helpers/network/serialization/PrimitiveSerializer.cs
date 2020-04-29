using System;
using Godot;

namespace Overlords.helpers.network.serialization
{
    public class PrimitiveSerializer<T>: SerializationCore.IHandler<T>
    {
        public readonly Func<T, bool> ValidateData;

        public PrimitiveSerializer()
        {
            ValidateData = null;
        }
        
        public PrimitiveSerializer(Func<T, bool> validateData)
        {
            ValidateData = validateData;
        }
            
        public object Serialize(T data)
        {
            if (!ValidateData(data))
                GD.PushWarning("Serializing object that is non-conformant to its own specification.");
            return data;
        }

        public object SerializeTypeless(object raw)
        {
            return Serialize((T) raw);
        }

        public T Deserialize(object raw)
        {
            return raw is T data && (ValidateData == null || ValidateData(data)) ? data :
                throw new SerializationCore.DeserializationException();
        }
    }
}