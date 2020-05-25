using Godot;
using Overlords.game.constants;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicLocal: Node
    {
        [RequireParent] public KinematicBody Body;
        [LinkNodeStatic("../FpsCamera")] public Camera Camera;
        
        public float Sensitivity => -Mathf.Deg2Rad(0.1F);
        public bool HasControl;
        
        public float RotHorizontal;
        public float RotVertical;
        public Vector3 Velocity;

        public float FrictionCoef = 0.7F;
        public const float MaxSpeed = 100F;
        public float MovementImpulse;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            GD.Print("This is our player!!! :)");
            Camera.Current = true;
            ApplyRotation();
            MovementImpulse = -(MaxSpeed * (Mathf.Log(FrictionCoef) / Mathf.Log(10))) / FrictionCoef;  // TODO: Fix my math!
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

            if (HasControl)
            {
                var heading = new Vector3();
                if (GameInputs.FpsForward.IsPressed()) heading += Vector3.Forward;
                if (GameInputs.FpsBackward.IsPressed()) heading += Vector3.Back;
                if (GameInputs.FpsLeftward.IsPressed()) heading += Vector3.Left;
                if (GameInputs.FpsRightward.IsPressed()) heading += Vector3.Right;
                Velocity += heading.Normalized().Rotated(Vector3.Up, RotHorizontal) * MovementImpulse;
            }
            Velocity *= FrictionCoef;
            Velocity = Body.MoveAndSlide(Velocity, Vector3.Up);
            GD.Print(Velocity.Length());
        }

        private void ApplyRotation()
        {
            Camera.Transform = new Transform(
                Basis.Identity.Rotated(Vector3.Right, RotVertical).Rotated(Vector3.Up, RotHorizontal),
                Camera.Transform.origin);
        }
    }
}