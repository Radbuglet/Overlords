using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.gameplay
{
    public class HumanoidBodyController: Node
    {
        [RequireParent] public KinematicBody Body;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        [Export] public readonly float Friction = 0.98F;
        [Export] public Vector3 Velocity;
        [Export] public Vector3 Gravity = Vector3.Down * 10;

        public void ApplyImpulse(Vector3 force)
        {
            Velocity += force;
        }

        public void Process(Vector3 desiredVelocity, float maxTorque)
        {
            var velocityHorizontal = Velocity.MoveToward(desiredVelocity / Friction, maxTorque);
            Velocity.x = velocityHorizontal.x;
            Velocity.z = velocityHorizontal.z;
            Velocity *= Friction;
            Velocity = Body.MoveAndSlide(Velocity + Vector3.Down * 0.1F, Vector3.Up);
            if (!Body.IsOnFloor())
                Velocity += Gravity;
        }
    }
}