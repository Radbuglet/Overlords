using Godot.Collections;
using Overlords.helpers.csharp;

namespace Overlords.helpers.network.serialization
{
    public class StructSerializer<TStruct>: AbstractSerializer<TStruct>
    {
        private readonly ObjectFactory<TStruct> _structFactory;
        private readonly System.Collections.Generic.Dictionary<string, ISerializerRaw> _fields;

        public StructSerializer(ObjectFactory<TStruct> structFactory, System.Collections.Generic.Dictionary<string, ISerializerRaw> fields)
        {
            _structFactory = structFactory;
            _fields = fields;
        }
        
        public override object Serialize(TStruct data)
        {
            var serializedArray = new Array();
            foreach (var configuredField in _fields)
            {
                serializedArray.Add(configuredField.Value.SerializeUnTyped(
                    data.GetValueSafe(configuredField.Key)));
            }

            return serializedArray;
        }

        public override TStruct Deserialize(object raw)
        {
            if (!(raw is Array serializedStructRoot) || _fields.Count != serializedStructRoot.Count)
                throw new DeserializationException($"Invalid primitive root type for struct.");
            var deserializedStruct = _structFactory();

            var index = 0;
            foreach (var field in _fields)
            {
                var deserializedFieldValue = field.Value.DeserializeUnTyped(serializedStructRoot[index]);
                deserializedStruct.SetValueSafe(field.Key, deserializedFieldValue);
                index++;
            }

            return deserializedStruct;
        }
    }
}