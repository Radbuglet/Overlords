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
    }
    
    public static class StructSerialization
    {
        public static object Serialize<TStruct>(TStruct instance, IEnumerable<SerializableStructField> fields)
        {
            var instanceType = typeof(TStruct);
            var raw = new Array();
            foreach (var field in fields)
            {
                raw.Add(field.GetReflectionField(instanceType).GetValue(instance));
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