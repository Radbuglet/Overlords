using System;
using System.Collections.Generic;
using Godot;
using Overlords.game.definitions;
using Overlords.helpers.network;

namespace Overlords.game.entities.player
{
    public class PlayerShared : Node, IQuarantinedListener
    {
        private PlayerRoot Root => GetNode<PlayerRoot>("../../");
        public NetObjectVariant MyVariant;
        
        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Client) return;
            this.FlagQuarantineListener();
        }

        public void OnSetupComplete()
        {
            // Shared initialization
            Root.AddToGroup(EntityTypes.PlayersGroupName);

            // Find and apply variant
            var variant = GetTree().GetNetworkVariant(Root.State.OwnerPeerId.Value);
            MyVariant = variant;
            variant.ApplyToTree(new Dictionary<NetObjectVariant, IEnumerable<Func<Node>>>
            {
                [NetObjectVariant.LocalAuthoritative] = new Func<Node>[]
                {
                    () => Root.FpsCamera,
                    () => Root.MovementLocal
                }
            });
            if (variant == NetObjectVariant.LocalAuthoritative)
                Root.FpsCamera.Current = true;
        }

        public void _QuarantineOver()
        {
            OnSetupComplete();
        }

        public bool ValidateOwnerOnlyRpc(string action)
        {
            var sender = GetTree().GetRpcSenderId();
            if (sender == Root.State.OwnerPeerId.Value) return false;
            GD.PushWarning($"RPC {action} can only be interacted with by the owner. Peer {sender} violated the rule.");
            return true;
        }
    }
}