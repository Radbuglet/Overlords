using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.network.replication
{
    public class StateReplicator : Node
    {
        public interface IField
        {
            int FieldIndex { get; set; }

            object SerializeData();
            void DeserializeRemoteValue(object raw);
        }
        
        private readonly List<IField> _fields = new List<IField>();

        public void RegisterField(IField field)
        {
            field.FieldIndex = _fields.Count;
            _fields.Add(field);
        }

        public void ReplicateValues(IEnumerable<int> targets, IEnumerable<IField> fields, bool reliable)
        {
            var serialized = SerializeValues(fields);

            foreach (var target in targets)
                if (reliable)
                    RpcId(target, nameof(_ValueRemotelySet), serialized);
                else
                    RpcUnreliableId(target, nameof(_ValueRemotelySet), serialized);
        }

        public Dictionary SerializeValues(IEnumerable<IField> fields)
        {
            var serialized = new Godot.Collections.Dictionary<int, object>();
            foreach (var field in fields)
                serialized.Add(field.FieldIndex, field.SerializeData());
            return (Dictionary) serialized;
        }

        public Dictionary SerializeValues()
        {
            return SerializeValues(_fields);
        }

        public Array SerializeValuesCatchup()
        {
            var serialized = new Array();
            foreach (var field in _fields)
                serialized.Add(field.SerializeData());
            return serialized;
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

        public void LoadValuesCatchup(Array array)
        {
            if (array.Count != _fields.Count)
            {
                throw new DeserializationException("Mismatched StateReplicator catchup packet length.");
            }
            
            for (var index = 0; index < _fields.Count; index++)
            {
                _fields[index].DeserializeRemoteValue(array[index]);
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