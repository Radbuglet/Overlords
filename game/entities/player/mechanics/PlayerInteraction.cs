using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree.trackingGroup;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerInteraction : Spatial
    {
        private const float MaxInteractDistance = 12;
        private PlayerRoot Root => GetNode<PlayerRoot>("../../");
        
        public void OnLocalInteract()
        {
            // Ray-cast
            var rayCast = Root.LookRayCast;
            rayCast.CastTo = Vector3.Forward * MaxInteractDistance;
            rayCast.ForceRaycastUpdate();
            
            // Find entity  TODO: Sound/animation for failing interaction
            if (!(rayCast.GetCollider() is Spatial target)) return;
            if (!target.GetIdInGroup(Root.WorldRoot.Shared.InteractionTargets, out var targetId)) return;

                // Replicate interaction
            this.RpcServer(nameof(_Interacted), targetId, rayCast.GetCollisionPoint() - target.GetGlobalPosition());
        }

        [Master]
        private void _Interacted(string entityId, Vector3 relative)
        {
            // Validate player and get target
            if (Root.SharedLogic.ValidateOwnerOnlyRpc(nameof(_Interacted))) return;
            var target = Root.WorldRoot.Shared.InteractionTargets.GetMemberOfGroup<Spatial>(entityId, null);
            if (target == null)
            {
                GD.PushWarning($"Player {Root.State.OwnerPeerId.Value} interacted with entity that didn't exist.");
                return;
            }

            // Validate interaction distance
            var fromPoint = Root.LookRayCast.GetGlobalPosition();
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