using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.player
{
    public class PlayerShared : Node, IMacroCatchupValidator
    {
        [Export] private float _sneakReductionCoef;
        private Vector3 _originalHeadPosition;
        
        private PlayerRoot Player => this.FindFirstAncestor<PlayerRoot>();
        
        // Setup
        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Client) return;
            this.FlagMacroValidator();
            _originalHeadPosition = Player.Head.Translation;
        }

        public void ValidateCatchupState()
        {
            OnSetupComplete();
        }

        public void OnSetupComplete()
        {
            // Find and apply variant
            var variant = GetTree().GetNetworkVariant(Player.State.OwnerPeerId);
            variant.ApplyToTree(new Dictionary<NetObjectVariant, IEnumerable<Func<Node>>>
            {
                [NetObjectVariant.LocalAuthoritative] = new Func<Node>[]
                {
                    () => Player.Camera,  // This also deletes LookRayCast
                    () => Player.ControlsLocal
                }
            });
            if (variant == NetObjectVariant.LocalAuthoritative)
                Player.Camera.Current = true;
        }

        // Common tasks
        public bool ValidateOwnerOnlyRpc(string action)
        {
            var sender = GetTree().GetRpcSenderId();
            if (sender == Player.State.OwnerPeerId) return false;
            GD.PushWarning($"RPC {action} can only be interacted with by the owner. Peer {sender} violated the rule.");
            return true;
        }

        public Vector3 GetHeadPosition(bool isSneaking)
        {
            return isSneaking ? _originalHeadPosition * _sneakReductionCoef : _originalHeadPosition;
        }

        public bool IsOverlord()
        {
            return Player.Game.State.OverlordId == Player.State.OwnerPeerId;
        }
    }
}