using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.world.logic
{
    public class ClientLogic : Node
    {
        public override void _Ready()
        {
            SetProcess(false);
            GetTree().Connect(SceneTreeSignals.ConnectedToServer, this, nameof(_Connected));
        }

        private void _Connected()
        {
            SetProcess(true);
        }

        public override void _Process(float delta)
        {
            if (!GetTree().PerformQuarantineSweep(out var problem))
            {
                GD.PushError(problem);
            }
        }
    }
}