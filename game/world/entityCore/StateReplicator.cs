using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Array = Godot.Collections.Array;

namespace Overlords.game.world.entityCore
{
    public abstract class StateReplicator : Node, IRequiresCatchup, IQuarantineInfectable
    {
        private readonly List<IReplicatedField> _fields = new List<IReplicatedField>();
        private bool _constructed;

        public override void _Ready()
        {
            if (GetTree().GetNetworkMode() == NetworkMode.Client)
                this.FlagQuarantineInfectable();
        }

        protected ReplicatedField<T> AddField<T>(bool isOneShot = false)
        {
            var field = new ReplicatedField<T>(_fields.Count, isOneShot);
            _fields.Add(field);
            return field;
        }

        protected void ReplicateField(IReplicatedField field)
        {
            foreach (var peerId in GetTree().GetPlayingPeers())
            {
                RpcId(peerId, nameof(_SetOneValue), field.Index, field.NetGetValue());
            }
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

        public void _QuarantineChecking()
        {
            if (!_constructed)
            {
                throw new QuarantineContamination("StateReplicator never received a valid initial state.");
            }
        }
    }

    public interface IReplicatedField
    {
        int Index { get; }
        bool NetSetValue(object raw);
        object NetGetValue();
    }

    public class ReplicatedField<T>: IReplicatedField
    {
        public int Index { get; }
        public readonly bool IsOneShot;
        public T Value;

        public ReplicatedField(int index, bool isOneShot)
        {
            Index = index;
            Value = default;
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
                GD.PushWarning("ReplicatedField had its value set to the wrong type.");
                return false;
            }
        }

        public object NetGetValue()
        {
            return Value;
        }
    }
}