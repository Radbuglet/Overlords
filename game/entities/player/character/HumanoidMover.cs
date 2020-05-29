using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class HumanoidMover: Node
    {
        [RequireParent] public KinematicBody Body;
        
        [Export] public Vector3 Velocity;
        [Export] public float TimeToAccelerate;
        [Export] public float FullSpeed;
        [Export] public float SneakSpeed;
        
        [Export] public float Gravity;
        [Export] public float JumpMagnitude;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void Move(float delta, bool isJumping, bool isSneaking, Vector3 horizontalHeading)
        {
            // TODO: Make acceleration a bit more realistic
            horizontalHeading.y = 0;
            var velocityHorizontal = Velocity.MoveToward(
                horizontalHeading.Normalized() * (isSneaking ? SneakSpeed : FullSpeed), FullSpeed / TimeToAccelerate * delta);
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