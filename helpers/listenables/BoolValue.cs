using Godot;

namespace Overlords.helpers.listenables
{
    public class BoolValue: BaseValue<bool>
    {
        [Export] protected override bool ContainedValue { get; set; }
    }
}