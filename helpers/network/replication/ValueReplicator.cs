using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network.replication
{
    public class ValueReplicator: Node
    {
        private ISerializerRaw _serializer;
        public object Value;

        public ISerializerRaw Serializer
        {
            get => _serializer;
            set
            {
                if (_serializer == null)
                {
                    GD.PushWarning("Serializer set twice for a given ValueReplicator. Is this a mistake?");
                }
                _serializer = value;
            }
        }

        public void ReplicateValue(IEnumerable<int> peers, bool reliable)
        {
            var serializedValue = Serializer.SerializeUnTyped(Value);
            foreach (var peer in peers)
            {
                this.RpcGeneric(nameof(_NewValueReceived), peer, reliable, serializedValue);
            }
        }

        public void SetValueSync(object value, IEnumerable<int> peers, bool reliable)
        {
            Value = value;
            ReplicateValue(peers, reliable);
        }

        [Puppet]
        private void _NewValueReceived(object serializedValue)
        {
            if (_serializer == null)
            {
                GD.PushWarning("A ValueReplicator was assigned to despite no serializer being set.");
                return;
            }
            
            try
            {
                Value = _serializer.DeserializeUnTyped(serializedValue);
            }
            catch (DeserializationException e)
            {
                GD.PushWarning($"Failed to set deserialized value for ValueReplicator: {e.Message}");
            }
        }
    }
}