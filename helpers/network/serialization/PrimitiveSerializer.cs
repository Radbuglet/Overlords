using System;
using Godot;

namespace Overlords.helpers.network.serialization
{
    public class PrimitiveSerializer<T>: SerializationCore.IUpCastableHandler<T>, SerializationCore.ITypelessHandler
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
            
        public object Serialize(T structData)
        {
            if (!ValidateData(structData))
                GD.PushWarning("Serializing object that is non-conformant to its own specification.");
            return structData;
        }

        public T Deserialize(object raw)
        {
            return raw is T data && (ValidateData == null || ValidateData(data)) ? data :
                throw new SerializationCore.DeserializationException();
        }
        
        public object SerializeTypeless(object raw)
        {
            return Serialize((T) raw);
        }
        
        public object DeserializeTypeless(object raw)
        {
            return Deserialize(raw);
        }
    }
}