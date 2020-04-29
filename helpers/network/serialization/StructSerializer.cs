using System;
using System.Collections.Generic;
using System.Reflection;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class StructSerializer<TStruct>: SerializationCore.IHandler<TStruct>
    {
        private readonly Func<TStruct> _emptyStructFactory;

        public StructSerializer(Func<TStruct> emptyStructFactory)
        {
            _emptyStructFactory = emptyStructFactory;
        }

        private IEnumerable<Tuple<FieldInfo, SerializationCore.IUnsafeHandler<object>>> IterateSubFields()
        {
            var type = typeof(TStruct);
            foreach (var field in type.GetFields())
            {
                var serializationAttr = field.GetCustomAttribute<SerializableStructField>();
                if (serializationAttr == null) continue;
                yield return new Tuple<FieldInfo, SerializationCore.IUnsafeHandler<object>>(
                    field, serializationAttr.SerializationProvider());
            }
        }
        
        public object Serialize(TStruct data)
        {
            var serializedResult = new Array();
            foreach (var (field, handler) in IterateSubFields())
            {
                serializedResult.Add(handler.SerializeTypeless(
                    field.GetValue(data)));
            }
            
            return serializedResult;
        }

        public object SerializeTypeless(object raw)
        {
            return Serialize((TStruct) raw);
        }

        public TStruct Deserialize(object raw)
        {
            if (!(raw is Array structRawData)) throw new SerializationCore.DeserializationException();
            var structInstance = _emptyStructFactory();
            var index = 0;
            foreach (var (field, handler) in IterateSubFields())
            {
                if (index >= structRawData.Count) throw new SerializationCore.DeserializationException();
                field.SetValue(structInstance, handler.Deserialize(structRawData[index]));
                index++;
            }
            return structInstance;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableStructField: Attribute
    {
        public readonly Func<SerializationCore.IUnsafeHandler<object>> SerializationProvider;
        public SerializableStructField(Func<SerializationCore.IUnsafeHandler<object>> serializationProvider)
        {
            SerializationProvider = serializationProvider;
        }
    }
}