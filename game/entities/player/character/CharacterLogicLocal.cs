using Godot;
using Overlords.game.constants;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicLocal: Node
    {
        [LinkNodeStatic("../FpsCamera")] public Camera Camera;

        public float Sensitivity => -Mathf.Deg2Rad(0.1F);
        public bool HasControl = false;
        
        public float RotHorizontal;
        public float RotVertical;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            GD.Print("This is our player!!! :)");
            Camera.Current = true;
            ApplyRotation();
        }

        public override void _Input(InputEvent ev)
        {
            if (!HasControl || !(ev is InputEventMouseMotion evMotion)) return;
            RotHorizontal += evMotion.Relative.x * Sensitivity;
            RotVertical += evMotion.Relative.y * Sensitivity;
            RotHorizontal -= Mathf.Floor(RotHorizontal / Mathf.Tau) * Mathf.Tau;
            RotVertical = Mathf.Clamp(RotVertical, -Mathf.Pi / 2, Mathf.Pi / 2);
            ApplyRotation();
        }

        public override void _Process(float delta)
        {
            if (GameInputs.DebugAttachControl.WasJustPressed())
            {
                HasControl = !HasControl;
                Input.SetMouseMode(HasControl ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            }
        }

        private void ApplyRotation()
        {
            Camera.Transform = new Transform(
                Basis.Identity.Rotated(Vector3.Right, RotVertical).Rotated(Vector3.Up, RotHorizontal),
                Camera.Transform.origin);
        }
    }
}