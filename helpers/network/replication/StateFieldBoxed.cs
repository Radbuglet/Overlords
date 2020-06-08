using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network.replication
{
    public class StateFieldBoxed<TValue>: StateReplicator.IField
    {
        public delegate void ValueChangedRemotely(TValue newValue, TValue oldValue);
        
        public int FieldIndex { get; set; } = -1;
        public TValue Value;
        private readonly ISerializer<TValue> _serializer;
        public event ValueChangedRemotely OnChangedRemotely;

        public StateFieldBoxed(ISerializer<TValue> serializer, StateReplicator replicator = null)
        {
            _serializer = serializer;
            replicator?.RegisterField(this);
        }

        public object SerializeData()
        {
            return _serializer.Serialize(Value);
        }

        public void DeserializeRemoteValue(object raw)
        {
            var newValue = _serializer.Deserialize(raw);  // Exception may be raised
            var oldValue = Value;
            Value = newValue;
            OnChangedRemotely?.Invoke(newValue, oldValue);
        }
    }
}