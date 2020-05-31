using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.network.replication
{
    public interface IStateField
    {
        int FieldIndex { get; }

        object SerializeData();
        void DeserializeRemoteValue(object raw);
    }

    public class StateField<TValue> : Node, IStateField
    {
        [Signal]
        public delegate void ValueChangedRemotely(TValue newValue, TValue oldValue);

        private readonly ISerializer<TValue> _serializer;
        private TValue _value;

        public StateField(ISerializer<TValue> serializer)
        {
            _serializer = serializer;
            this.AddUserSignals(); // Because of Godot jank, the signal attribute isn't applied on anonymous nodes.
        }

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

        public int FieldIndex { get; set; } = -1;

        public object SerializeData()
        {
            return _serializer.Serialize(Value);
        }

        public void DeserializeRemoteValue(object raw)
        {
            if (!_serializer.TryDeserializedOrWarn(raw, out var newValue)) return;
            var oldValue = Value;
            Value = newValue;
            EmitSignal(nameof(ValueChangedRemotely), newValue, oldValue);
        }
    }

    public class StateReplicator : Node
    {
        private readonly List<IStateField> _fields = new List<IStateField>();

        public StateField<TVal> MakeField<TVal>(ISerializer<TVal> serializer)
        {
            var stateField = new StateField<TVal>(serializer);
            AddChild(stateField);
            stateField.FieldIndex = _fields.Count;
            _fields.Add(stateField);
            return stateField;
        }

        public void SetValueReplicated<T>(IEnumerable<int> targets, StateField<T> field, T value, bool reliable)
        {
            field.Value = value;
            ReplicateValues(targets, field.AsEnumerable(), reliable);
        }

        public void ReplicateValues(IEnumerable<int> targets, IEnumerable<IStateField> fields, bool reliable)
        {
            var serialized = SerializeValues(fields);

            foreach (var target in targets)
                if (reliable)
                    RpcId(target, nameof(_ValueRemotelySet), serialized);
                else
                    RpcUnreliableId(target, nameof(_ValueRemotelySet), serialized);
        }

        public Dictionary SerializeValues(IEnumerable<IStateField> fields)
        {
            var serialized = new Godot.Collections.Dictionary<int, object>();
            foreach (var field in fields) serialized.Add(field.FieldIndex, field.SerializeData());

            return (Dictionary) serialized;
        }

        public Dictionary SerializeValues()
        {
            return SerializeValues(_fields);
        }

        public void LoadValues(Dictionary rawDictionary)
        {
            foreach (var key in rawDictionary.Keys)
            {
                if (!(key is int fieldIndex) || fieldIndex < 0 || fieldIndex >= _fields.Count)
                {
                    GD.PushWarning("Failed to deserialize fieldIndex for state set.");
                    continue;
                }

                _fields[fieldIndex].DeserializeRemoteValue(rawDictionary[key]);
            }
        }

        [Puppet]
        private void _ValueRemotelySet(object raw)
        {
            if (!(raw is Dictionary rawDictionary))
            {
                GD.PushWarning($"Invalid packet root type for {nameof(_ValueRemotelySet)}.");
                return;
            }

            LoadValues(rawDictionary);
        }
    }
}