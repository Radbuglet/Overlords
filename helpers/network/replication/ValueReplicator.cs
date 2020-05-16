using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network.serialization;

namespace Overlords.helpers.network.replication
{
    public class ValueReplicator<TValue>: Node, IReplicatorCatchesUp
    {
        [Signal] public delegate void ValueRemotelyUpdated(TValue value);
        
        private ISerializer<TValue> _serializer;
        public TValue Value;

        public ISerializer<TValue> Serializer
        {
            get => _serializer;
            set
            {
                if (_serializer != null)
                {
                    GD.PushWarning("Serializer set twice for a given ValueReplicator. Is this a mistake?");
                }

                if (value == null)
                {
                    GD.PushWarning($"{nameof(ValueReplicator<TValue>)}'s serializer was set to null!");
                }
                _serializer = value;
            }
        }

        public override void _Ready()
        {
            this.RegisterCatchupReceiver();
        }

        public void ReplicateValue(IEnumerable<int> peers, bool reliable)
        {
            var serializedValue = Serializer.Serialize(Value);
            foreach (var peer in peers)
            {
                this.RpcGeneric(nameof(_NewValueReceived), peer, reliable, serializedValue);
            }
        }

        public void SetValueSync(TValue value, IEnumerable<int> peers, bool reliable)
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
                var oldValue = Value;
                Value = _serializer.Deserialize(serializedValue);
                EmitSignal(nameof(ValueRemotelyUpdated), Value, oldValue);
            }
            catch (DeserializationException e)
            {
                GD.PushWarning($"Failed to set deserialized value for ValueReplicator: {e.Message}");
            }
        }

        public IEnumerable<Node> CatchupJoinedPeer(int targetPeerId)
        {
            ReplicateValue(targetPeerId.AsEnumerable(), true);
            return null;
        }

        public static ValueReplicator<TValue> MakeReplicator(Node bindTo, string networkName)
        {
            var value = new ValueReplicator<TValue>
            {
                Name = networkName
            };
            bindTo.AddChild(value);
            return value;
        }
    }
}