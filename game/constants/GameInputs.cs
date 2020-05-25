namespace Overlords.game.constants
{
    public static class GameInputs
    {
        public static readonly Action FpsForward = new Action("fps_forward");
        public static readonly Action FpsBackward = new Action("fps_backward");
        public static readonly Action FpsLeftward = new Action("fps_leftward");
        public static readonly Action FpsRightward = new Action("fps_rightward");
        public static readonly Action FpsJump = new Action("fps_jump");
        public static readonly Action DebugAttachControl = new Action("debug_attach_control");
        
        public class Action
        {
            public readonly string Id;

            public Action(string id)
            {
                Id = id;
            }

            public bool IsPressed()
            {
                return Godot.Input.IsActionPressed(Id);
            }

            public bool WasJustPressed()
            {
                return Godot.Input.IsActionJustPressed(Id);
            }
            
            public bool WasJustReleased()
            {
                return Godot.Input.IsActionJustReleased(Id);
            }
        }
    }
}