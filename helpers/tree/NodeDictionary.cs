using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.csharp;

namespace Overlords.helpers.tree
{
    /// <summary>
    /// A reference object that behaves like a normal dictionary and automatically removes any nodes that leave the scene tree.
    /// </summary>
    public class NodeDictionary<TKey, TValue> : Reference where TValue : Node
    {
        private readonly Dictionary<TKey, TValue> _idToElement = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TValue, TKey> _elementToId = new Dictionary<TValue, TKey>();
        
        /// <summary>
        /// Adds a node element to the dictionary. A given node may only be added to a dictionary once. Same goes for keys.
        /// </summary>
        public void Add(TKey key, TValue element)
        {
            Debug.Assert(!_idToElement.ContainsKey(key) && !_elementToId.ContainsKey(element));
            _idToElement.Add(key, element);
            _elementToId.Add(element, key);
            element.Connect(NodeSignals.TreeExited, this, nameof(_ElementRemoved), new Godot.Collections.Array{ key, element });
        }

        /// <summary>
        /// Removes a node from the dictionary by its key. Returns true if the node existed and false if it did not.
        /// </summary>
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