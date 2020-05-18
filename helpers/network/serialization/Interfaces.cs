using System;

namespace Overlords.helpers.network.serialization
{
    public class SerializationException: Exception
    {
        public SerializationException() {}
        public SerializationException(string reason): base(reason) {}
    }
    
    public class DeserializationException: Exception
    {
        public DeserializationException() {}
        public DeserializationException(string reason): base(reason) {}
    }
    
    public interface ISerializer<T>
    {
        object Serialize(T data);
        T Deserialize(object raw);
    }

    public interface ISerializerRaw
    {
        object SerializeUnTyped(object data);
        object DeserializeUnTyped(object raw);
    }

    public abstract class AbstractSerializer<T>: ISerializer<T>, ISerializerRaw
    {
        public abstract object Serialize(T data);
        public abstract T Deserialize(object raw);

        public object SerializeUnTyped(object data)
        {
            if (!(data is T dataCasted))
                throw new SerializationException("Data provided to non-typed serialization variant was of an invalid C# type! " +
                                                 $"Provided type: {(data == null ? "<null>" : data.GetType().Name)} Expected type: {typeof(T).Name}");
            return Serialize(dataCasted);
        }

        public object DeserializeUnTyped(object raw)
        {
            return Deserialize(raw);
        }
    }
}