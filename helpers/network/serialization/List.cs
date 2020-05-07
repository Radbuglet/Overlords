using System.Collections.Generic;
using Godot.Collections;

namespace Overlords.helpers.network.serialization
{
    public static class ListSerialization
    {
        public static object SerializeTyped<T>(IEnumerable<T> array, CoreSerialization.ITypelessSerializationPair serializer)
        {
            var serializedArray = new Array();
            foreach (var element in array)
            {
                serializedArray.Add(serializer.SerializeTypeless(element));
            }
            return serializedArray;
        }

        public static Array<T> DeserializeTyped<T>(object raw, CoreSerialization.ITypelessSerializationPair serializer)
        {
            var deserializedArray = new Array<T>();
            var rawArray = PrimitiveSerialization.Deserialize<Array>(raw);
            foreach (var rawElement in rawArray)
            {
                deserializedArray.Add((T) serializer.DeserializeTypeless(rawElement));
            }
            return deserializedArray;
        }

        public static TypelessSerializationPair MakePair<TVal>(CoreSerialization.ITypelessSerializationPair serializer)
        {
            return TypelessSerializationPair.OfPair(serializer, SerializeTyped, DeserializeTyped<TVal>);
        }
    }
}