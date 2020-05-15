using System.Collections.Generic;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class ListSerializer<TElem>: AbstractSerializer<Array<TElem>>
    {
        private readonly ISerializer<TElem> _elementSerializer;
        
        public ListSerializer(ISerializer<TElem> elementSerializer)
        {
            _elementSerializer = elementSerializer;
        }
        
        public override object Serialize(Array<TElem> data)
        {
            return ListSerialization.Serialize(_elementSerializer, data);
        }

        public override Array<TElem> Deserialize(object serializedRaw)
        {
            return ListSerialization.Deserialize(_elementSerializer, serializedRaw);
        }
    }

    public static class ListSerialization
    {
        public static object Serialize<TElem>(ISerializer<TElem> elementSerializer, IEnumerable<TElem> data)
        {
            var serializedArray = new Array();
            foreach (var element in data)
            {
                serializedArray.Add(elementSerializer.Serialize(element));
            }
            
            return serializedArray;
        }
        
        public static Array<TElem> Deserialize<TElem>(ISerializer<TElem> elementSerializer, object serializedRaw)
        {
            var deserializedArray = new Array<TElem>();
            if (!(serializedRaw is Array serializedArray))
                throw new DeserializationException("Root of serialized typed list was not a primitive array!");
            foreach (var serializedElement in serializedArray)
            {
                deserializedArray.Add(elementSerializer.Deserialize(serializedElement));
            }

            return deserializedArray;
        }
    }
}