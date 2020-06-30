using Godot;
using Overlords.game.definitions;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerControlsLocal : Node, IValidationAwaiter
    {
        private Vector2 _rotation;
        
        public PlayerRoot Root => GetNode<PlayerRoot>("../../");
        private float Sensitivity => -Mathf.Deg2Rad(0.2f);
        private bool HasControl => Input.GetMouseMode() == Input.MouseMode.Captured;

        public override void _Ready()
        {
            this.FlagAwaiter();
        }
        
        public void _CatchupStateValidated()
        {
            ApplyRotation();
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        private void ApplyRotation()
        {
            Root.FpsCamera.Transform = new Transform(
                Basis.Identity
                    .Rotated(Vector3.Right, _rotation.y)
                    .Rotated(Vector3.Up, _rotation.x),
                Root.FpsCamera.Translation);
        }

        public override void _Input(InputEvent @event)
        {
            if (!HasControl || !(@event is InputEventMouseMotion motion)) return;
            _rotation += motion.Relative * Sensitivity;
            _rotation.y = Mathf.Clamp(_rotation.y, -Mathf.Pi / 2, Mathf.Pi / 2);
            _rotation.x -= Mathf.Floor(_rotation.x / Mathf.Tau) * Mathf.Tau;
            ApplyRotation();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (GameInputs.FpsPause.WasJustPressed())
            {
                Input.SetMouseMode(HasControl ? Input.MouseMode.Visible : Input.MouseMode.Captured);
            }
            
            // Generate heading
            var heading = new Vector3();
            var isSneaking = HasControl && GameInputs.FpsSneak.IsPressed();
            if (HasControl)
            {
                if (GameInputs.FpsForward.IsPressed()) heading += Vector3.Forward;
                if (GameInputs.FpsBackward.IsPressed()) heading += Vector3.Back;
                if (GameInputs.FpsLeftward.IsPressed()) heading += Vector3.Left;
                if (GameInputs.FpsRightward.IsPressed()) heading += Vector3.Right;
                heading = heading.Rotated(Vector3.Up, _rotation.x);

                if (GameInputs.FpsInteract.WasJustPressed())
                    Root.Interaction.OnLocalInteract(isSneaking);
            }

            // Perform movement
            Root.Mover.Move(delta, heading, HasControl && GameInputs.FpsJump.IsPressed(),
                isSneaking, false);
            Root.MovementNet.ReplicateMyPosition();
            
            // Update head and camera
            var head = Root.Head;
            var targetHeadPosition = Root.SharedLogic.GetHeadPosition(isSneaking);
            head.Translation = (head.Translation + targetHeadPosition) / 2.0f;
        }
    }
}