using Godot;

namespace Overlords.helpers.listenables
{
    public class StringValue: BaseValue<string>
    {
        [Export] protected override string ContainedValue { get; set; } = "";
    }
}