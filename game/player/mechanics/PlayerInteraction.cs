using Godot;
using Overlords.game.props;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.player.mechanics
{
    public class PlayerInteraction : Spatial
    {
        private const float MaxInteractDistance = 12;
        private PlayerRoot Player => this.FindFirstAncestor<PlayerRoot>();
        
        public void OnLocalInteract(bool isSneaking)
        {
            // Ray-cast
            var rayCast = Player.LookRayCast;
            rayCast.CastTo = Vector3.Forward * MaxInteractDistance;
            rayCast.ForceRaycastUpdate();
            
            // Find entity  TODO: Sound/animation for failing interaction
            if (!(rayCast.GetCollider() is Spatial target)) return;
            if (!Player.Game.InteractTargets.TryGetKey(target, out var targetId)) return;

            // Replicate interaction
            GD.Print("Attempting to interact...");
            this.RpcMaster(nameof(_Interacted), targetId, rayCast.GetCollisionPoint() - target.GetGlobalPosition(), isSneaking);
        }

        [Master]
        private void _Interacted(string entityId, Vector3 relative, bool isSneaking)
        {
            // TODO: Interaction when player is looking down directly is buggy ("failed to interact with the object (obscured?)").
            // Validate player and get target
            if (Player.Shared.ValidateOwnerOnlyRpc(nameof(_Interacted))) return;
            if (!Player.Game.InteractTargets.TryGetValue<Spatial>(entityId, out var target))
            {
                GD.PushWarning($"Player {Player.State.OwnerPeerId} interacted with entity that didn't exist.");
                return;
            }

            // Validate interaction distance
            Player.Head.Translation = Player.Shared.GetHeadPosition(isSneaking);
            var fromPoint = Player.Head.GetGlobalPosition();
            var targetPoint = target.GetGlobalPosition() + relative;
            var pointDistance = fromPoint.DistanceTo(targetPoint);
            if (pointDistance > MaxInteractDistance)
            {
                GD.PushWarning($"Player {Player.State.OwnerPeerId} interacted with an object that was too far away.");
                return;
            }

            // Add some margin to the segment
            targetPoint += (targetPoint - fromPoint) / pointDistance * 0.1f;
            
            // RayCast, check, and fire locally
            var result = GetWorld().DirectSpaceState.IntersectRay(fromPoint, targetPoint);
            if (result == null || !result.Contains("collider") || result["collider"] != target)
            {
                GD.PushWarning($"Player {Player.State.OwnerPeerId} failed to interact with the object (obscured?).");
                return;
            }
            
            ((IProp) target)._OnObjectInteracted(Player);
        }
    }
}