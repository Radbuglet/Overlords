using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.world.entityCore
{
    public abstract class StateReplicator : Node, IRequiresCatchup, IInvariantEnforcer
    {
        private readonly List<IReplicatedField> _fields = new List<IReplicatedField>();
        private bool _constructed;

        public override void _Ready()
        {
            if (GetTree().GetNetworkMode() == NetworkMode.Client)
                this.FlagEnforcer();
        }

        protected ReplicatedField<T> AddField<T>(bool isOneShot = false)
        {
            var field = new ReplicatedField<T>(_fields.Count, isOneShot);
            _fields.Add(field);
            return field;
        }

        protected void ReplicateField(IReplicatedField field)
        {
            foreach (var peerId in this.GetWorldRoot().Shared.GetOnlinePeers())
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

            return new CatchupState(packet, true);
        }

        public void HandleCatchupState(object valuesRaw)
        {
            if (_constructed)
            {
                GD.PushWarning($"Initial values have already been provided to the {nameof(StateReplicator)}.");
                return;
            }

            if (!(valuesRaw is Array values))
            {
                GD.PushWarning("StateReplicator failed to handle catchup state: root wasn't an array!");
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

        public void ValidateCatchupState(SceneTree tree)
        {
            if (!_constructed)
            {
                throw new InvalidCatchupException("StateReplicator never received a valid initial state.");
            }
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