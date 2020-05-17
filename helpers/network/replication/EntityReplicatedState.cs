using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network.replication
{
    public class EntityReplicatedState: Node
    {
        public interface IStateField
        {
            int FieldIndex { get; }
            
            object SerializeData();
            void DeserializeValue(object raw);
        }
        
        public class StateField<TValue>: IStateField
        {
            public int FieldIndex { get; set; }
            
            private readonly ISerializer<TValue> _serializer;
            private TValue _value;
            public bool IsValueSet { get; private set; }

            public TValue Value
            {
                get => _value;
                set
                {
                    IsValueSet = true;
                    _value = value;
                }
            }

            public StateField(ISerializer<TValue> serializer)
            {
                _serializer = serializer;
            }

            public object SerializeData()
            {
                return _serializer.Serialize(Value);
            }

            public void DeserializeValue(object raw)
            {
                try
                {
                    Value = _serializer.Deserialize(raw);
                }
                catch (DeserializationException e)
                {
                    GD.PushWarning($"Failed to set StateField: {e.Message}");
                }
            }
        }

        private readonly List<IStateField> _fields = new List<IStateField>();

        public StateField<TVal> AddField<TVal>(StateField<TVal> stateField)
        {
            Debug.Assert(stateField.FieldIndex == -1);
            stateField.FieldIndex = _fields.Count;
            _fields.Add(stateField);
            return stateField;
        }

        public void ReplicateValues(IEnumerable<int> targets, IEnumerable<IStateField> fields, bool reliable)
        {
            var serialized = SerializeValues(fields);
            
            foreach (var target in targets)
            {
                if (reliable)
                    RpcId(target, nameof(RemotelySetValues), serialized);
                else
                    RpcUnreliableId(target, nameof(RemotelySetValues), serialized);
            }
        }

        public object SerializeValues(IEnumerable<IStateField> fields)
        {
            var serialized = new Godot.Collections.Dictionary<int, object>();
            foreach (var field in fields)
            {
                serialized.Add(field.FieldIndex, field.SerializeData());
            }

            return serialized;
        }

        public void LoadValues(Dictionary rawDictionary)
        {
            foreach (KeyValuePair<object, object> kv in rawDictionary)
            {
                if (!(kv.Key is int fieldIndex) || fieldIndex < 0 || fieldIndex >= _fields.Count)
                {
                    GD.PushWarning("Failed to deserialize fieldIndex for state set.");
                    continue;
                }
                
                _fields[fieldIndex].DeserializeValue(kv.Value);
            }
        }

        [Puppet]
        private void RemotelySetValues(object raw)
        {
            if (!(raw is Dictionary rawDictionary))
            {
                GD.PushWarning($"Invalid packet root type for {nameof(RemotelySetValues)}.");
                return;
            }

            LoadValues(rawDictionary);
        }
    }
}