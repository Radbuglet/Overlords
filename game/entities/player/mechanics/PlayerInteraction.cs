using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerInteraction : Spatial
    {
        private const float MaxInteractDistance = 12;
        private PlayerRoot Root => GetNode<PlayerRoot>("../../");
        
        public void OnLocalInteract(bool isSneaking)
        {
            // Ray-cast
            var rayCast = Root.LookRayCast;
            rayCast.CastTo = Vector3.Forward * MaxInteractDistance;
            rayCast.ForceRaycastUpdate();
            
            // Find entity  TODO: Sound/animation for failing interaction
            if (!(rayCast.GetCollider() is Spatial target)) return;
            if (!Root.WorldRoot.Shared.InteractionTargets.TryGetKey(target, out var targetId)) return;

            // Replicate interaction
            this.RpcServer(nameof(_Interacted), targetId, rayCast.GetCollisionPoint() - target.GetGlobalPosition(), isSneaking);
        }

        [Master]
        private void _Interacted(string entityId, Vector3 relative, bool isSneaking)
        {
            // TODO: Interaction when player is looking down directly is buggy ("failed to interact with the object (obscured?)").
            // Validate player and get target
            if (Root.SharedLogic.ValidateOwnerOnlyRpc(nameof(_Interacted))) return;
            if (!Root.WorldRoot.Shared.InteractionTargets.TryGetValue<Spatial>(entityId, out var target))
            {
                GD.PushWarning($"Player {Root.State.OwnerPeerId.Value} interacted with entity that didn't exist.");
                return;
            }

            // Validate interaction distance
            Root.Head.Translation = Root.SharedLogic.GetHeadPosition(isSneaking);
            var fromPoint = Root.Head.GetGlobalPosition();
            var targetPoint = target.GetGlobalPosition() + relative;
            var pointDistance = fromPoint.DistanceTo(targetPoint);
            if (pointDistance > MaxInteractDistance)
            {
                GD.PushWarning($"Player {Root.State.OwnerPeerId.Value} interacted with an object that was too far away.");
                return;
            }

            // Add some margin to the segment
            targetPoint += (targetPoint - fromPoint) / pointDistance * 0.1f;
            
            // RayCast, check, and fire locally
            var result = GetWorld().DirectSpaceState.IntersectRay(fromPoint, targetPoint);
            if (result == null || !result.Contains("collider") || result["collider"] != target)
            {
                GD.PushWarning($"Player {Root.State.OwnerPeerId.Value} failed to interact with the object (obscured?).");
                return;
            }
            
            target.EmitSignal(nameof(EntityTypes.SignalOnInteracted), Root);
        }
    }
}