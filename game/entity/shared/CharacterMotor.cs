using Godot;
using Overlords.helpers.behaviors;

#pragma warning disable 649

namespace Overlords.game.entity.shared
{
    public class CharacterMotor : Node
    {
        [RequireParent] private KinematicBody _body;
        [Export] public float Acceleration = 20;
        [Export] public float Gravity = 9.8f;
        [Export] public float HorizontalFriction = 0.8f;
        [Export] public float JumpMagnitude = 70;
        [Export] public Vector3 Velocity = Vector3.Zero;
        [Export] public float VerticalFriction = 0.98f;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void Move(float rotateHorizontal, Vector2 heading, bool jump)
        {
            // Make movement vector
            var movement = new Vector3();
            movement += Vector3.Forward.Rotated(Vector3.Up, rotateHorizontal) * heading.x;
            movement += Vector3.Right.Rotated(Vector3.Up, rotateHorizontal) * heading.y;
            movement = movement.Normalized() * Acceleration;

            // Update velocity
            Velocity += movement;
            Velocity *= new Vector3(HorizontalFriction, VerticalFriction, HorizontalFriction);

            // Move and handle
            _body.MoveAndSlide(Velocity, Vector3.Up);
            if (_body.IsOnFloor())
            {
                if (jump) Velocity.y = JumpMagnitude;
            }
            else
            {
                Velocity.y -= Gravity;
            }
        }
    }
}