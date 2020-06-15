using Godot;

namespace Overlords.game.definitions
{
    public static class GameInputs
    {
        public static readonly Action FpsForward = new Action("fps_forward");
        public static readonly Action FpsBackward = new Action("fps_backward");
        public static readonly Action FpsLeftward = new Action("fps_leftward");
        public static readonly Action FpsRightward = new Action("fps_rightward");
        public static readonly Action FpsJump = new Action("fps_jump");
        public static readonly Action FpsSneak = new Action("fps_sneak");
        public static readonly Action FpsInteract = new Action("fps_interact");
        public static readonly Action FpsInventory = new Action("fps_inventory");
        public static readonly Action FpsPause = new Action("fps_pause");

        public class Action
        {
            public readonly string Id;

            public Action(string id)
            {
                Id = id;
            }

            public bool IsPressed()
            {
                return Input.IsActionPressed(Id);
            }

            public bool WasJustPressed()
            {
                return Input.IsActionJustPressed(Id);
            }

            public bool WasJustReleased()
            {
                return Input.IsActionJustReleased(Id);
            }
        }
    }
}