using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class HumanoidMover : Node
    {
        [RequireParent] public KinematicBody Body;
        
        [Export] public float AccelTimeAir;
        [Export] public float AccelTimeGround;
        [Export] public float FullSpeed;
        [Export] public float Gravity;
        [Export] public float JumpMagnitude;
        [Export] public float SneakSpeed;
        [Export] public Vector3 Velocity;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void Move(float delta, bool isJumping, bool isSneaking, Vector3 horizontalHeading)
        {
            // TODO: Add acceleration curves; integrate positions to avoid FPS based de-syncs
            horizontalHeading.y = 0;
            var velocityHorizontal = Velocity.MoveToward(
                horizontalHeading.Normalized() * (isSneaking ? SneakSpeed : FullSpeed),
                FullSpeed / (Body.IsOnFloor() ? AccelTimeGround : AccelTimeAir) * delta);
            Velocity.x = velocityHorizontal.x;
            Velocity.z = velocityHorizontal.z;

            Velocity.y -= Gravity * delta;
            Velocity = Body.MoveAndSlide(Velocity, Vector3.Up);

            // ReSharper disable once InvertIf
            if (Body.IsOnFloor())
            {
                Velocity.y = 0;
                if (isJumping)
                    Velocity.y = JumpMagnitude;
            }
        }
    }
}