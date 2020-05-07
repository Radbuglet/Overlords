using System;
using System.Collections.Generic;
using System.Diagnostics;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.serialization
{
    public class EnumValueSplitter<TMeta, TEnum>
    {
        private readonly Dictionary<TEnum, Action<TMeta, object>> _handlers = new Dictionary<TEnum, Action<TMeta, object>>();

        public Array Encode(TEnum type, object value)
        {
            return new Array{ type, value };
        }

        public void ProcessDecoding(TMeta meta, object raw)
        {
            if (!(raw is Array rawArray) || rawArray.Count != 2 || !(rawArray[0] is int))
                throw new CoreSerialization.DeserializationException("Invalid hub packet root.");

            var enumRaw = rawArray[0];
            if (!Enum.IsDefined(typeof(TEnum), enumRaw))
                throw new CoreSerialization.DeserializationException("Invalid hub packet type: invalid enum value");

            if (!_handlers.TryGetValue((TEnum) enumRaw, out var packetHandler))
                throw new CoreSerialization.DeserializationException("Invalid hub packet type: no handler bound");
            
            packetHandler(meta, rawArray[1]);
        }
        
        public void BindDecodingHandler(TEnum target, Action<TMeta, object> handler)
        {
            Debug.Assert(!_handlers.ContainsKey(target));
            _handlers.Add(target, handler);
        }
    }
}