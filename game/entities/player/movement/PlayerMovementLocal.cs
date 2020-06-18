using Godot;
using Overlords.game.definitions;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.movement
{
    public class PlayerMovementLocal : Node, IQuarantinedListener
    {
        public PlayerRoot Root => GetNode<PlayerRoot>("../../../");
        private float Sensitivity => -Mathf.Deg2Rad(0.2f);
        private Vector2 _rotation;

        public override void _Ready()
        {
            this.FlagQuarantineListener();
        }
        
        public void _QuarantineOver()
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
            if (!(@event is InputEventMouseMotion motion)) return;
            _rotation += motion.Relative * Sensitivity;
            _rotation.y = Mathf.Clamp(_rotation.y, -Mathf.Pi / 2, Mathf.Pi / 2);
            _rotation.x -= Mathf.Floor(_rotation.x / Mathf.Tau) * Mathf.Tau;
            ApplyRotation();
        }

        public override void _PhysicsProcess(float delta)
        {
            var heading = new Vector3();
            if (GameInputs.FpsForward.IsPressed()) heading += Vector3.Forward;
            if (GameInputs.FpsBackward.IsPressed()) heading += Vector3.Back;
            if (GameInputs.FpsLeftward.IsPressed()) heading += Vector3.Left;
            if (GameInputs.FpsRightward.IsPressed()) heading += Vector3.Right;
            heading = heading.Rotated(Vector3.Up, _rotation.x);
            
            Root.Mover.Move(delta, heading, GameInputs.FpsJump.IsPressed(),
                GameInputs.FpsSneak.IsPressed(), false);
            Root.MovementNet.ReplicateMyPosition();
        }
    }
}