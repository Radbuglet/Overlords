namespace Overlords.game.definitions
{
    public interface ISerializableEntity
    {
        (int typeId, object constructor) SerializeConstructor();
    }
}