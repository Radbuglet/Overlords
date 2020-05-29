using Godot;
using Overlords.game.constants;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicLocal: Node
    {
        [RequireParent] public KinematicBody Body;
        [LinkNodeStatic("../FpsCamera")] public Camera Camera;
        [RequireBehavior] public HumanoidMover Mover;
        
        public float Sensitivity => -Mathf.Deg2Rad(0.1F);
        public bool HasControl;
        
        public float RotHorizontal;
        public float RotVertical;
        public Vector3 Velocity;

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

        public override void _PhysicsProcess(float delta)
        {
            if (GameInputs.DebugAttachControl.WasJustPressed())
            {
                HasControl = !HasControl;
                Input.SetMouseMode(HasControl ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            }
            
            var heading = new Vector3();
            if (HasControl)
            {
                if (GameInputs.FpsForward.IsPressed()) heading += Vector3.Forward;
                if (GameInputs.FpsBackward.IsPressed()) heading += Vector3.Back;
                if (GameInputs.FpsLeftward.IsPressed()) heading += Vector3.Left;
                if (GameInputs.FpsRightward.IsPressed()) heading += Vector3.Right;
                heading = heading.Rotated(Vector3.Up, RotHorizontal);
            }
            Mover.Move(delta, HasControl && GameInputs.FpsJump.IsPressed(), HasControl && GameInputs.FpsSneak.IsPressed(), heading);
        }

        private void ApplyRotation()
        {
            Camera.Transform = new Transform(
                Basis.Identity.Rotated(Vector3.Right, RotVertical).Rotated(Vector3.Up, RotHorizontal),
                Camera.Transform.origin);
        }
    }
}