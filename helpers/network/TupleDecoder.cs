using Godot.Collections;

namespace Overlords.helpers.network
{
    public struct TupleDecoder
    {
        private readonly Array _array;
        private int _index;
        
        public TupleDecoder(object packet)
        {
            _array = packet is Array array ? array : null;
            _index = 0;
        }

        public bool Read<T>(out T value)
        {
            if (_array == null || _index >= _array.Count)
            {
                value = default;
                return false;
            }

            var valueRaw = _array[_index];
            if (!(valueRaw is T))
            {
                value = default;
                return false;
            }

            value = (T) valueRaw;
            _index++;
            return true;
        }
    }
}