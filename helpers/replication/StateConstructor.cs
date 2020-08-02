using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.network;

namespace Overlords.helpers.replication
{
    public abstract class StateConstructor : Node, IRequiresCatchup
    {
        public delegate void ValueSetterNoValidation<in T>(T raw);
        public delegate bool ValueSetter<in T>(T raw);
        public delegate T ValueGetter<out T>();
        
        private struct ReplicatedField
        {
            public ValueSetter<object> Setter;
            public ValueGetter<object> Getter;
        }
        
        private readonly List<ReplicatedField> _fields = new List<ReplicatedField>();

        public override void _Ready()
        {
            this.FlagRequiresCatchup();
        }

        private static bool ValidateValue<T>(object raw, bool isNullable, out T value)
        {
            if (raw is T)
            {
                value = (T) raw;
                return true;
            }
            value = default;
            return isNullable && raw == null;
        }
        
        public void AddFieldValidated<T>(ValueGetter<T> getter, ValueSetter<T> setter, bool isNullable = false)
        {
            _fields.Add(new ReplicatedField
            {
                Setter = raw => ValidateValue(raw, isNullable, out T value) && setter(value),
                Getter = () => getter()
            });
        }
        
        public void AddField<T>(ValueGetter<T> getter, ValueSetterNoValidation<T> setter, bool isNullable = false)
        {
            _fields.Add(new ReplicatedField
            {
                Setter = raw =>
                {
                    if (!ValidateValue(raw, isNullable, out T value)) return false;
                    setter(value);
                    return true;
                },
                Getter = () => getter()
            });
        }

        public object CatchupOverNetwork(int peerId)
        {
            var packet = new Array();
            foreach (var field in _fields)
            {
                packet.Add(field.Getter());
            }

            return packet;
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
                if (!field.Setter(values[index]))
                    throw new InvalidCatchupException("Invalid value for field no. " + index + " in StateReplicator.");
                index++;
            }
        }
    }
}