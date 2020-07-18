using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;

namespace Overlords.helpers.tree
{
    public class NodeDictionary<TKey, TValue> : Reference where TValue : Node
    {
        private readonly Dictionary<TKey, TValue> _idToElement = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TValue, TKey> _elementToId = new Dictionary<TValue, TKey>();
        
        public void Add(TKey key, TValue element)
        {
            _idToElement.Add(key, element);
            _elementToId.Add(element, key);
            element.Connect(NodeSignals.TreeExited, this, nameof(_ElementRemoved), new Godot.Collections.Array{ key, element });
        }

        public bool Remove(TKey key)
        {
            if (!_idToElement.TryGetValue(key, out var element))
                return false;
            element.Disconnect(NodeSignals.TreeExited, this, nameof(_ElementRemoved));
            _idToElement.Remove(key);
            _elementToId.Remove(element);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue element)
        {
            return _idToElement.TryGetValue(key, out element);
        }

        public bool TryGetKey(TValue element, out TKey key)
        {
            return _elementToId.TryGetValue(element, out key);
        }

        public bool TryGetValue<T>(TKey key, out T element) where T : TValue
        {
            var found = TryGetValue(key, out var elementRaw);
            element = elementRaw as T;
            return found;
        }

        public ICollection<TKey> GetKeys()
        {
            return _idToElement.Keys;
        }

        public ICollection<TValue> GetValues()
        {
            return _idToElement.Values;
        }

        private void _ElementRemoved(TKey key, TValue element)
        {
            _idToElement.Remove(key);
            _elementToId.Remove(element);
        }
    }
}