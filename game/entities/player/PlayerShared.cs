using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network;

namespace Overlords.game.entities.player
{
    public class PlayerShared : Node, IValidationAwaiter
    {
        [Export] private float _sneakReductionCoef;
        private Vector3 _originalHeadPosition;
        
        private PlayerRoot Root => GetNode<PlayerRoot>("../../");
        
        // Setup
        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Client) return;
            this.FlagAwaiter();
            _originalHeadPosition = Root.Head.Translation;
        }
        
        public void _StateValidated()
        {
            OnSetupComplete();
        }

        public void OnSetupComplete()
        {
            // Find and apply variant
            var variant = GetTree().GetNetworkVariant(Root.State.OwnerPeerId.Value);
            variant.ApplyToTree(new Dictionary<NetObjectVariant, IEnumerable<Func<Node>>>
            {
                [NetObjectVariant.LocalAuthoritative] = new Func<Node>[]
                {
                    () => Root.FpsCamera,  // This also deletes LookRayCast
                    () => Root.ControlsLocal
                }
            });
            if (variant == NetObjectVariant.LocalAuthoritative)
                Root.FpsCamera.Current = true;
        }

        // Common tasks
        public bool ValidateOwnerOnlyRpc(string action)
        {
            var sender = GetTree().GetRpcSenderId();
            if (sender == Root.State.OwnerPeerId.Value) return false;
            GD.PushWarning($"RPC {action} can only be interacted with by the owner. Peer {sender} violated the rule.");
            return true;
        }

        public Vector3 GetHeadPosition(bool isSneaking)
        {
            return isSneaking ? _originalHeadPosition * _sneakReductionCoef : _originalHeadPosition;
        }

        public bool IsOverlord()
        {
            return Root.WorldRoot.State.OverlordId.Value == Root.State.OwnerPeerId.Value;
        }
    }
}