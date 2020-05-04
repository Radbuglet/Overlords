using System;
using System.Collections.Generic;
using System.Reflection;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public struct SerializableStructField
    {
        private readonly string _fieldName;
        public readonly Func<object, object> Serialize;
        public readonly Func<object, object> Deserialize;

        public SerializableStructField(string fieldName, Func<object, object> serialize, Func<object, object> deserialize)
        {
            _fieldName = fieldName;
            Serialize = serialize;
            Deserialize = deserialize;
        }

        public FieldInfo GetReflectionField(Type type)
        {
            return type.GetField(_fieldName);
        }

        public static SerializableStructField OfPair<TData>(string fieldName,
            CoreSerialization.Serialize<TData> serialize, CoreSerialization.Deserialize<TData> deserialize)
        {
            return new SerializableStructField(fieldName, data => serialize((TData) data),
                raw => deserialize(raw));
        }
        
        public static SerializableStructField OfPair<TData, TConfig>(string fieldName, TConfig config,
            CoreSerialization.SerializeConfigured<TData, TConfig> serialize, CoreSerialization.DeserializeConfigured<TData, TConfig> deserialize)
        {
            return new SerializableStructField(fieldName, data => serialize((TData) data, config),
                raw => deserialize(raw, config));
        }

        public static SerializableStructField OfPrimitive<TValue>(string fieldName)
        {
            return new SerializableStructField(fieldName, data => PrimitiveSerialization.Serialize((TValue) data),
                raw => PrimitiveSerialization.Deserialize<TValue>(raw));
        }
        
        public static SerializableStructField OfStruct<TStruct>(string fieldName, SimpleStructSerializer<TStruct> serializer)
        {
            return OfPair(fieldName, serializer.Serialize, serializer.Deserialize);
        }
    }
    
    public class SimpleStructSerializer<TStruct>
    {
        private readonly Func<TStruct> _createEmptyStruct;
        private readonly Func<IEnumerable<SerializableStructField>> _fieldGetter;

        public SimpleStructSerializer(Func<TStruct> emptyStruct, Func<IEnumerable<SerializableStructField>> fieldGetter)
        {
            _createEmptyStruct = emptyStruct;
            _fieldGetter = fieldGetter;
        }

        public object Serialize(TStruct instance)
        {
            return StructSerialization.Serialize(instance, _fieldGetter());
        }
        
        public TStruct Deserialize(object raw)
        {
            return StructSerialization.Deserialize(raw, (_fieldGetter(), _createEmptyStruct()));
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
                raw.Add(field.Serialize(
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
                field.GetReflectionField(instanceType).SetValue(targetInstance, field.Deserialize(rawField));
                index++;
            }
            return targetInstance;
        }
    }
}