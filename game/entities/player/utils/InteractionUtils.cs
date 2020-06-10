using Godot;
using Overlords.game.entities.player.local;
using Overlords.helpers.csharp;

namespace Overlords.game.entities.player.utils
{
    public static class InteractionUtils
    {
        public const float MaxInteractionDistance = 6;
        
        public static bool PerformInteraction(this PlayerLocal player, float maxDistance, out Spatial hitBody, out Vector3 hitPointRelative)
        {
            var lookRayCast = player.LogicShared.InteractionRayCast;
            lookRayCast.CastTo = Vector3.Forward * maxDistance;
            lookRayCast.ForceRaycastUpdate();
            
            hitBody = lookRayCast.GetCollider() as Spatial;
            hitPointRelative = lookRayCast.GetCollisionPoint();
            
            // ReSharper disable once InvertIf
            if (hitBody != null)
            {
                hitPointRelative -= hitBody.GetGlobalPosition();  // G = b + r  ;  r = G - b
                return true;
            }
            return false;
        }
        
        public static bool ValidateInteraction(this PlayerServer player, Spatial hitBody, Vector3 hitPointRelative, float maxDistance)
        {
            var fromPoint = player.LogicShared.InteractionRayCast.GetGlobalPosition();
            var toPoint = hitBody.GetGlobalPosition() + hitPointRelative;
            if (fromPoint.DistanceTo(toPoint) > maxDistance) return false;
            toPoint += fromPoint.DirectionTo(toPoint) * 0.1F;
            var intersection = player.GetWorld().DirectSpaceState.IntersectRay(fromPoint, toPoint);
            return intersection != null && intersection["collider"] == hitBody;
        }
    }
}