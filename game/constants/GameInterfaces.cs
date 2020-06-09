namespace Overlords.game.constants
{
    public interface ISerializableEntity
    {
        (int typeId, object constructor) SerializeConstructor();
    }
}