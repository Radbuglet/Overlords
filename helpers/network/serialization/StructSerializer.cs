using System;
using System.Collections.Generic;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class StructSerializer<TStruct>: SerializationCore.IUpCastableHandler<TStruct>, SerializationCore.ITypelessHandler where TStruct: ISerializableStruct
    {
        private readonly Func<TStruct> _emptyStructFactory;

        public StructSerializer(Func<TStruct> emptyStructFactory)
        {
            _emptyStructFactory = emptyStructFactory;
        }

        public object Serialize(TStruct structData)
        {
            var structDataType = typeof(TStruct);
            var dataSerialized = new Array();
            foreach (var (name, serializer) in structData.GetSerializedFields())
            {
                var fieldValue = structDataType.GetField(name).GetValue(structData);
                dataSerialized.Add(serializer.SerializeTypeless(fieldValue));
            }
            return dataSerialized;
        }

        public TStruct Deserialize(object raw)
        {
            if (!(raw is Array structRawData)) throw new SerializationCore.DeserializationException();
            var structInstance = _emptyStructFactory();
            var structDataType = typeof(TStruct);
            var index = 0;
            foreach (var (fieldName, serializer) in structInstance.GetSerializedFields())
            {
                if (index >= structRawData.Count) throw new SerializationCore.DeserializationException();
                structDataType.GetField(fieldName)
                    .SetValue(structInstance, serializer.DeserializeTypeless(structRawData[index]));
                index++;
            }
            return structInstance;
        }
        
        public object SerializeTypeless(object raw)
        {
            return Serialize((TStruct) raw);
        }

        public object DeserializeTypeless(object raw)
        {
            return Deserialize(raw);
        }
    }

    public struct SerializableStructField
    {
        public readonly string FieldName;
        public readonly SerializationCore.ITypelessHandler Serializer;

        public SerializableStructField(string fieldName, SerializationCore.ITypelessHandler serializer)
        {
            FieldName = fieldName;
            Serializer = serializer;
        }
        
        public void Deconstruct(out string fieldName, out SerializationCore.ITypelessHandler serializer)
        {
            fieldName = FieldName;
            serializer = Serializer;
        }
    }

    public interface ISerializableStruct
    {
        IEnumerable<SerializableStructField> GetSerializedFields();
    }
}