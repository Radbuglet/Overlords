using Godot;
using Overlords.helpers.network;

namespace Overlords.game.world.logic
{
    public class ClientLogic : Node
    {
        public override void _Process(float delta)
        {
            if (!GetTree().PerformQuarantineSweep(out var problem))
            {
                GD.PushError(problem);
            }
        }
    }
}