﻿using System;
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
            var variant = GetTree().GetNetworkVariant(Player.State.OwnerPeerId.Value);
            variant.ApplyToTree(new Dictionary<NetObjectVariant, IEnumerable<Func<Node>>>
            {
                [NetObjectVariant.FlagAuthoritative] = new Func<Node>[]
                {
                    () => Player.Inventory
                },
                [NetObjectVariant.LocalAuthoritative] = new Func<Node>[]
                {
                    () => Player.FpsCamera,  // This also deletes LookRayCast
                    () => Player.ControlsLocal
                }
            });
            if (variant == NetObjectVariant.LocalAuthoritative)
                Player.FpsCamera.Current = true;
        }

        // Common tasks
        public bool ValidateOwnerOnlyRpc(string action)
        {
            var sender = GetTree().GetRpcSenderId();
            if (sender == Player.State.OwnerPeerId.Value) return false;
            GD.PushWarning($"RPC {action} can only be interacted with by the owner. Peer {sender} violated the rule.");
            return true;
        }

        public Vector3 GetHeadPosition(bool isSneaking)
        {
            return isSneaking ? _originalHeadPosition * _sneakReductionCoef : _originalHeadPosition;
        }

        public bool IsOverlord()
        {
            // That's a lot of redirects. At least the component system was better in that regard.
            return Player.Game.State.OverlordId.Value == Player.State.OwnerPeerId.Value;
        }
    }
}