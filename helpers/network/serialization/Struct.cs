using System;
using System.Collections.Generic;
using System.Reflection;
using static Overlords.helpers.network.serialization.CoreSerialization;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class SimpleStructSerializer<TStruct>: ITypelessSerializationPair
    {
        private readonly Func<TStruct> _createEmptyStruct;
        private readonly IEnumerable<SerializableStructField> _fields;

        public SimpleStructSerializer(Func<TStruct> emptyStruct, IEnumerable<SerializableStructField> fields)
        {
            _createEmptyStruct = emptyStruct;
            _fields = fields;
        }

        public object Serialize(TStruct instance)
        {
            return StructSerialization.Serialize(instance, _fields);
        }
        
        public TStruct Deserialize(object raw)
        {
            return StructSerialization.Deserialize(raw, (_fields, _createEmptyStruct()));
        }

        public object SerializeTypeless(object data)
        {
            return Serialize((TStruct) data);
        }

        public object DeserializeTypeless(object raw)
        {
            return Deserialize(raw);
        }
    }

    public class SerializableStructField
    {
        private readonly string _fieldName;
        public readonly ITypelessSerializationPair SerializationPair;

        public SerializableStructField(string fieldName, ITypelessSerializationPair serializationPair)
        {
            _fieldName = fieldName;
            SerializationPair = serializationPair;
        }

        public FieldInfo GetReflectionField(Type type)
        {
            return type.GetField(_fieldName);
        }
    }

    public static class StructSerialization
    {
        public static object Serialize<TStruct>(TStruct instance, IEnumerable<SerializableStructField> fields)
        {
            var instanceType = typeof(TStruct);
            var raw = new Array();
            foreach (var field in fields)
            {
                raw.Add(field.SerializationPair.SerializeTypeless(
                    field.GetReflectionField(instanceType).GetValue(instance)));
            }
            return raw;
        }
        
        public static TStruct Deserialize<TStruct>(object raw, (IEnumerable<SerializableStructField> fields, TStruct targetInstance) config)
        {
            var instanceType = typeof(TStruct);
            var (fields, targetInstance) = config;
            
            var rawArray = PrimitiveSerialization.Deserialize<Array>(raw);
            var index = 0;
            foreach (var field in fields)
            {
                var rawField = rawArray[index];
                field.GetReflectionField(instanceType).SetValue(targetInstance, field.SerializationPair.DeserializeTypeless(rawField));
                index++;
            }
            return targetInstance;
        }

        public static SerializableStructField ForField(this ITypelessSerializationPair serializationPair, string fieldName)
        {
            return new SerializableStructField(fieldName, serializationPair);
        }
    }
}