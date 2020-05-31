using System;

namespace Overlords.helpers.network.serialization
{
    public class PrimitiveSerializer<TElem> : AbstractSerializer<TElem>
    {
        private readonly Func<TElem, bool> _customValidate;

        public PrimitiveSerializer(Func<TElem, bool> customValidate)
        {
            _customValidate = customValidate;
        }

        public PrimitiveSerializer()
        {
            _customValidate = _ => true;
        }

        public override object Serialize(TElem data)
        {
            if (!_customValidate(data))
                throw new SerializationException("Serialized primitive doesn't conform to own format.");
            return data;
        }

        public override TElem Deserialize(object raw)
        {
            if (!(raw is TElem casted))
                throw new DeserializationException("Deserialized primitive doesn't match expected primitive type.");
            return casted;
        }
    }
}