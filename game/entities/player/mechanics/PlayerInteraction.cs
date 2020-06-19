using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerInteraction : Node
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
            if (!target.IsInGroup(EntityTypes.InteractableGroupName)) return;

            // Replicate interaction
            this.RpcServer(nameof(_Interacted), target.Name, rayCast.GetCollisionPoint() - target.GetGlobalPosition());
        }

        [Master]
        private void _Interacted(string entityName, Vector3 relative)
        {
            if (Root.SharedLogic.ValidateOwnerOnlyRpc(nameof(_Interacted))) return;
            GD.Print("Cool interaction!");
        }
    }
}