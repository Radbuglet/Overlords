using Godot;

namespace Overlords.helpers.listenables
{
    public class IntValue: BaseValue<int>
    {
        [Export] protected override int ContainedValue { get; set; }
    }
}