using System;

namespace Overlords.helpers.network.serialization
{
    public static class EnumSerialization
    {
        public static int SerializeEnum(this Enum value)
        {
            return (int) (object) value;
        }

        public static TEnum DeserializeEnum<TEnum>(this object serializedData) where TEnum : Enum
        {
            return serializedData is TEnum value ? value : throw new DeserializationException("Invalid enum value.");
        }
    }
}