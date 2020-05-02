using System;

namespace Overlords.helpers.network.serialization
{
    public class EnumSerializer<TEnum>: SerializationCore.IUpCastableHandler<TEnum>, SerializationCore.ITypelessHandler where TEnum: Enum
    {
        public TEnum Deserialize(object raw)
        {
            if (!(raw is int enumValueRaw) || !Enum.IsDefined(typeof(TEnum), enumValueRaw))
                throw new SerializationCore.DeserializationException();
            // ReSharper disable once PossibleInvalidCastException  TODO: Is this ok?
            return (TEnum) (object) enumValueRaw;
        }

        public object Serialize(TEnum structData)
        {
            return (int) (object) structData;
        }
        
        public object DeserializeTypeless(object raw)
        {
            return Deserialize(raw);
        }
        
        public object SerializeTypeless(object raw)
        {
            return Serialize((TEnum) raw);
        }
    }
}