namespace Overlords.game.constants
{
    public static class InputNames
    {
        public static readonly Action FpsForward = new Action("fps_forward");
        public static readonly Action FpsBackward = new Action("fps_backward");
        public static readonly Action FpsLeftward = new Action("fps_leftward");
        public static readonly Action FpsRightward = new Action("fps_rightward");
        public static readonly Action FpsJump = new Action("fps_jump");
        
        public class Action
        {
            private readonly string _actionName;

            public Action(string actionName)
            {
                _actionName = actionName;
            }

            public bool IsPressed()
            {
                return Godot.Input.IsActionPressed(_actionName);
            }

            public bool WasJustPressed()
            {
                return Godot.Input.IsActionJustPressed(_actionName);
            }
            
            public bool WasJustReleased()
            {
                return Godot.Input.IsActionJustReleased(_actionName);
            }
        }
    }
}