using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.helpers.replication
{
    public abstract class StateReplicator : Node, IRequiresCatchup
    {
        private readonly List<IReplicatedField> _fields = new List<IReplicatedField>();

        public override void _Ready()
        {
            this.FlagRequiresCatchup();
        }

        protected ReplicatedField<T> AddField<T>(bool isOneShot = false, bool isNullable = false, ReplicatedField<T>.ValueValidator validator = null)
        {
            var field = new ReplicatedField<T>(_fields.Count, isOneShot, isNullable, validator);
            _fields.Add(field);
            return field;
        }

        protected void ReplicateField(IReplicatedField field)
        {
            foreach (var peerId in this.EnumerateNetworkViewers())
            {
                RpcId(peerId, nameof(_SetOneValue), field.Index, field.NetGetValue());
            }
        }

        public CatchupState CatchupOverNetwork(int peerId)
        {
            var packet = new Array();
            foreach (var field in _fields)
            {
                packet.Add(field.NetGetValue());
            }

            return new CatchupState(packet);
        }

        public void HandleCatchupState(object valuesRaw)
        {
            if (!(valuesRaw is Array values))
            {
                throw new InvalidCatchupException("StateReplicator failed to handle catchup state: root wasn't an array!");
            }

            if (_fields.Count != values.Count)
            {
                throw new InvalidCatchupException("Value catchup packet field count does not match local field count.");
            }

            var index = 0;
            foreach (var field in _fields)
            {
                if (!field.NetSetValue(values[index], true))
                    throw new InvalidCatchupException("Invalid field value for StateReplicator.");
                index++;
            }
        }

        [Puppet]
        private void _SetOneValue(int index, object value)
        {
            if (this.DoesRequireCatchup())
            {
                GD.PushWarning($"Single value was set before {nameof(StateReplicator)} was constructed.");
                return;
            }
            if (!_fields.TryGetValue(index, out var field))
            {
                GD.PushWarning($"Unknown field with index {index}. Expected an index between 0 and {_fields.Count - 1} inclusive.");
                return;
            }
            field.NetSetValue(value, false);
        }
    }

    public interface IReplicatedField
    {
        int Index { get; }
        bool NetSetValue(object raw, bool isFirstTime);
        object NetGetValue();
    }

    public class ReplicatedField<T>: IReplicatedField
    {
        public delegate void ValueUpdateHandler(T newValue, T oldValue);
        public delegate bool ValueValidator(T value, ref string reason);
        
        public event ValueUpdateHandler ValueChanged;
        
        public int Index { get; }
        public readonly bool IsOneShot;
        public readonly bool IsNullable;
        public readonly ValueValidator Validator;
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = value;
                ValueChanged?.Invoke(value, oldValue);
            }
        } 

        public ReplicatedField(int index, bool isOneShot, bool isNullable, ValueValidator validator)
        {
            Index = index;
            Value = default;
            IsOneShot = isOneShot;
            IsNullable = isNullable;
            Validator = validator;
        }

        public bool NetSetValue(object raw, bool isFirstTime)
        {
            // Ensure OneShot policy
            if (!isFirstTime && IsOneShot)
            {
                GD.PushWarning("OneShot ReplicatedField was set multiple times.");
                return false;
            }

            // Parse type
            T newValue;
            if (raw is T value)
            {
                newValue = value;
            } else if (raw == null && IsNullable)
            {
                newValue = default;
            }
            else
            {
                GD.PushWarning("ReplicatedField had its value set to the wrong type.");
                return false;
            }
            
            // Validate value and set
            string problem = null;
            if (Validator != null && !Validator(newValue, ref problem))
            {
                GD.PushWarning($"ReplicatedValue was set to some invalid value: {problem ?? "(no reason given)"}");
                return false;
            }

            Value = newValue;
            return true;
        }

        public object NetGetValue()
        {
            return Value;
        }
    }
}