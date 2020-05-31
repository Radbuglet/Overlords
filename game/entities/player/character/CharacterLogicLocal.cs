using Godot;
using Overlords.game.constants;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.character
{
    public class CharacterLogicLocal: Node
    {
        [LinkNodeStatic("../FpsCamera")] public Camera Camera;
        [RequireBehavior] public HumanoidMover Mover;
        [RequireBehavior] public CharacterLogicShared LogicShared;
        
        private RemoteEventHub<CharacterProtocol.ClientBound, CharacterProtocol.ServerBound> _remoteEventHub;
        
        public float Sensitivity => -Mathf.Deg2Rad(0.1F);
        public bool HasControl;
        private Vector3 _initialCameraPos;
        
        public float RotHorizontal;
        public float RotVertical;

        public override void _Ready()
        {
            this.InitializeBehavior();
            ApplyRotation();
            
            Camera.Current = true;
            _initialCameraPos = Camera.Translation;
            _remoteEventHub = new RemoteEventHub<CharacterProtocol.ClientBound, CharacterProtocol.ServerBound>(
                LogicShared.RemoteEvent);
            AddChild(_remoteEventHub);
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

            var isSneaking = HasControl && GameInputs.FpsSneak.IsPressed();
            Mover.Move(delta, HasControl && GameInputs.FpsJump.IsPressed(), isSneaking, heading);
            
            Camera.Translation = (Camera.Translation + 0.4F * (isSneaking ? _initialCameraPos * 0.67F : _initialCameraPos)) / 1.4F;
            _remoteEventHub.FireServer((CharacterProtocol.ServerBound.PerformMovement, (object) Mover.Body.Translation));
        }

        private void ApplyRotation()
        {
            Camera.Transform = new Transform(
                Basis.Identity.Rotated(Vector3.Right, RotVertical).Rotated(Vector3.Up, RotHorizontal),
                Camera.Transform.origin);
        }
    }
}