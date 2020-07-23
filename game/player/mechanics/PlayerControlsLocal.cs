using Godot;
using Overlords.game.player.gui;
using Overlords.helpers.tree;

namespace Overlords.game.player.mechanics
{
    public class PlayerControlsLocal : Node
    {
        private Vector2 _rotation;
        
        public PlayerRoot Root => this.FindFirstAncestor<PlayerRoot>();
        private float Sensitivity => -Mathf.Deg2Rad(0.2f);
        private bool HasControl => Root.GuiController.State == GuiState.Playing;

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
            // Generate heading
            var heading = new Vector3();
            var isSneaking = HasControl && Input.IsActionPressed("fps_sneak");
            if (HasControl)
            {
                if (Input.IsActionPressed("fps_forward")) heading += Vector3.Forward;
                if (Input.IsActionPressed("fps_backward")) heading += Vector3.Back;
                if (Input.IsActionPressed("fps_leftward")) heading += Vector3.Left;
                if (Input.IsActionPressed("fps_rightward")) heading += Vector3.Right;
                heading = heading.Rotated(Vector3.Up, _rotation.x);

                if (Input.IsActionPressed("fps_interact"))
                    Root.Interaction.OnLocalInteract(isSneaking);
            }

            // Perform movement
            Root.Mover.Move(delta, heading, HasControl && Input.IsActionPressed("fps_jump"),
                isSneaking, false);
            Root.MovementNet.ReplicateMyPosition();
            
            // Update head and camera
            var head = Root.Head;
            var targetHeadPosition = Root.Shared.GetHeadPosition(isSneaking);
            head.Translation = (head.Translation + targetHeadPosition) / 2.0f;
        }
    }
}