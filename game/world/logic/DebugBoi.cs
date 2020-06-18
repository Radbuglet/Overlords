using Godot;

namespace Overlords.game.world.logic
{
    public class DebugBoi : Node
    {
        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                GetParent().PrintTree();
            }
        }
    }
}