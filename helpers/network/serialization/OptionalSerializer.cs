namespace Overlords.helpers.network.serialization
{
    public class OptionalSerializer<T> : AbstractSerializer<T>
    {
        private readonly ISerializer<T> _serializer;

        public OptionalSerializer(ISerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public override object Serialize(T data)
        {
            return data == null ? null : _serializer.Serialize(data);
        }

        public override T Deserialize(object raw)
        {
            return raw == null ? default : _serializer.Deserialize(raw);
        }
    }
}