using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.common;
using Overlords.game.entities.player.common;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;
using _EventHub = Overlords.helpers.network.RemoteEventHub<
    Overlords.game.entities.player.common.PlayerProtocol.ClientBound,
    Overlords.game.entities.player.common.PlayerProtocol.ServerBound>;

namespace Overlords.game.entities.player.local
{
    public class PlayerLocal: Node
    {
        [LinkNodeStatic("../FpsCamera")] public Camera Camera;
        [RequireBehavior] public PlayerShared LogicShared;
        [RequireBehavior] public HumanoidMover Mover;
        
        private _EventHub _remoteEventHub;
        private Vector3 _initialCameraPos;
        public bool HasControl;
        public float RotHorizontal;
        public float RotVertical;
        public float Sensitivity => -Mathf.Deg2Rad(0.1F);
        
        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            
            this.InitializeBehavior();
            ApplyRotation();
            Camera.Current = true;
            _initialCameraPos = Camera.Translation;
            _remoteEventHub = new _EventHub(LogicShared.RemoteEvent);
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
            // Handle pause menu
            if (GameInputs.DebugAttachControl.WasJustPressed())
            {
                HasControl = !HasControl;
                Input.SetMouseMode(HasControl ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            }

            // Generate heading; handle interact command
            var heading = new Vector3();
            if (HasControl)
            {
                if (GameInputs.FpsForward.IsPressed()) heading += Vector3.Forward;
                if (GameInputs.FpsBackward.IsPressed()) heading += Vector3.Back;
                if (GameInputs.FpsLeftward.IsPressed()) heading += Vector3.Left;
                if (GameInputs.FpsRightward.IsPressed()) heading += Vector3.Right;
                if (GameInputs.FpsInteract.WasJustPressed() && this.PerformInteraction(InteractionUtils.MaxInteractionDistance,
                    out var hitBody, out var hitPointRelative))
                {
                    if (hitBody.GetIdInGroup(LogicShared.GetWorldShared().InteractionTargets, out var interactionKey))
                    {
                        _remoteEventHub.FireServer((PlayerProtocol.ServerBound.Interact, new PlayerProtocol.InteractPacket
                        {
                            TargetId = interactionKey,
                            InteractPoint = hitPointRelative
                        }.Serialize()));
                    }
                }
                heading = heading.Rotated(Vector3.Up, RotHorizontal);
            }
            
            // Move player (with replication)
            var isSneaking = HasControl && GameInputs.FpsSneak.IsPressed();
            Mover.Move(delta, HasControl && GameInputs.FpsJump.IsPressed(), isSneaking, heading);
            
            Camera.Translation = (Camera.Translation + 0.4F * (isSneaking ? _initialCameraPos * 0.67F : _initialCameraPos)) / 1.4F;
            _remoteEventHub.FireUnreliableServer((PlayerProtocol.ServerBound.PerformMovement, (object) Mover.Body.Translation));
        }

        private void ApplyRotation()
        {
            Camera.Transform = new Transform(
                Basis.Identity.Rotated(Vector3.Right, RotVertical).Rotated(Vector3.Up, RotHorizontal),
                Camera.Transform.origin);
        }
    }
}