using System;
using System.Collections.Generic;
using Overlords.helpers.csharp;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class StructSerializer<TStruct> : AbstractSerializer<TStruct>
    {
        private readonly Dictionary<string, ISerializerRaw> _fields;
        private readonly ObjectFactory<TStruct> _structFactory;
        private readonly Action<TStruct> _validator;

        public StructSerializer(ObjectFactory<TStruct> structFactory, Dictionary<string, ISerializerRaw> fields)
        {
            _structFactory = structFactory;
            _fields = fields;
        }

        public StructSerializer(ObjectFactory<TStruct> structFactory, Dictionary<string, ISerializerRaw> fields,
            Action<TStruct> validator)
        {
            _structFactory = structFactory;
            _fields = fields;
            _validator = validator;
        }

        public override object Serialize(TStruct data)
        {
            var serializedArray = new Array();
            foreach (var configuredField in _fields)
                serializedArray.Add(configuredField.Value.SerializeUnTyped(
                    data.GetValueSafe(configuredField.Key)));

            return serializedArray;
        }

        public override TStruct Deserialize(object raw)
        {
            if (!(raw is Array serializedStructRoot) || _fields.Count != serializedStructRoot.Count)
                throw new DeserializationException(
                    $"Invalid primitive root type for struct. Expected array, got {(raw == null ? "<null>" : raw.GetType().Name)}");
            var deserializedStruct = _structFactory();

            var index = 0;
            foreach (var field in _fields)
            {
                var deserializedFieldValue = field.Value.DeserializeUnTyped(serializedStructRoot[index]);
                deserializedStruct.SetValueSafe(field.Key, deserializedFieldValue);
                index++;
            }

            _validator?.Invoke(deserializedStruct);
            return deserializedStruct;
        }
    }
}