using Godot;
using Overlords.helpers.tree;

namespace Overlords.game.shared
{
    public class HumanoidMover : Node
    {
        [Export] public NodePath PathToBody;
        [LinkNodeEditor(nameof(PathToBody))] public KinematicBody Body;

        [Export] public float AccelTimeAir;
        [Export] public float AccelTimeGround;
        [Export] public float FullSpeed;
        [Export] public float Gravity;
        [Export] public float JumpMagnitude;
        [Export] public float SneakSpeed;
        [Export] public Vector3 Velocity;

        public override void _Ready()
        {
            this.Initialize();
        }

        public void Move(float delta, Vector3 horizontalHeading, bool isJumping, bool isSneaking, bool airJumpAllowed)
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
                Velocity.y = 0;
            
            if ((Body.IsOnFloor() || airJumpAllowed) && isJumping)
                Velocity.y = JumpMagnitude;
        }
    }
}