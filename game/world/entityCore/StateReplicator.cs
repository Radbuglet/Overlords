using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.network.catchup;
using Overlords.helpers.tree.initialization;
using Array = Godot.Collections.Array;

namespace Overlords.game.world.entityCore
{
    public abstract class StateReplicator : Node, IRequiresCatchup
    {
        private readonly List<IReplicatedField> _fields = new List<IReplicatedField>();
        private bool _constructed;

        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Client) return;
            GetParent().ConnectOrCreate(nameof(Quarantine.QuarantineChecking),
                this, nameof(ValidateQuarantineState));
        }

        public void AddField<T>(bool isOneShot = false)
        {
            var index = _fields.Count;
            var field = new ReplicatedField<T>(isOneShot);
            if (this.GetNetworkMode() == NetworkMode.Server)
            {
                field.ValueChanged += (newValue, oldValue) =>
                {
                    foreach (var peerId in GetTree().GetPlayingPeers())
                    {
                        RpcId(peerId, nameof(_SetOneValue), index, newValue);
                    }
                };
            }
            _fields.Add(field);
        }
        
        public void CatchupState(int peerId)
        {
            var packet = new Array();
            foreach (var field in _fields)
            {
                packet.Add(field.NetGetValue());
            }

            RpcId(peerId, nameof(_CatchupInitialValues), packet);
        }

        private void ValidateQuarantineState()
        {
            if (!_constructed)
            {
                throw new QuarantineContamination("StateReplicator never received a valid initial state.");
            }
        }

        [Puppet]
        private void _CatchupInitialValues(Array values)
        {
            if (_constructed)
            {
                GD.PushWarning($"Initial values have already been provided to the {nameof(StateReplicator)}.");
                return;
            }

            if (_fields.Count != values.Count)
            {
                GD.PushWarning("Value catchup packet field count does not match local field count.");
                return;
            }

            var index = 0;
            foreach (var field in _fields)
            {
                if (!field.NetSetValue(values[index])) return;
                index++;
            }

            _constructed = true;
        }
        
        [Puppet]
        private void _SetOneValue(int index, object value)
        {
            if (!_constructed)
            {
                GD.PushWarning($"Single value was set before {nameof(StateReplicator)} was constructed.");
                return;
            }
            if (!_fields.TryGetValue(index, out var field))
            {
                GD.PushWarning($"Unknown field with index {index}. Expected an index between 0 and {_fields.Count - 1} inclusive.");
                return;
            }
            field.NetSetValue(value);
        }
    }

    public interface IReplicatedField
    {
        bool NetSetValue(object raw);
        object NetGetValue();
    }

    public class ReplicatedField<T>: IReplicatedField
    {
        public delegate void ValueChangeHandler(T newValue, T oldValue);
        
        public readonly bool IsOneShot;
        public event ValueChangeHandler ValueChanged;

        private T _valueInternal;
        public T Value
        {
            get => _valueInternal;
            set
            {
                var oldValue = _valueInternal;
                _valueInternal = value;
                ValueChanged?.Invoke(value, oldValue);
            }
        }

        public ReplicatedField(bool isOneShot)
        {
            _valueInternal = default;
            IsOneShot = isOneShot;
        }

        public bool NetSetValue(object raw)
        {
            if (raw is T newValue)
            {
                Value = newValue;
                return true;
            }
            else
            {
                GD.PushWarning("ReplicatedField had its value set to the wrong type. Ignored.");
                return false;
            }
        }

        public object NetGetValue()
        {
            return Value;
        }
    }
}