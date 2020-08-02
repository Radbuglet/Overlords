using System.Diagnostics;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.player.mechanics
{
    public class PlayerMovementNet : Node, IRequiresCatchup
    {
        public PlayerRoot Player => this.FindFirstAncestor<PlayerRoot>();

        public override void _Ready()
        {
            this.FlagRequiresCatchup();
        }

        public object CatchupOverNetwork(int peerId)
        {
            return Player.GetGlobalPosition();
        }
        
        public void HandleCatchupState(object argsRoot)
        {
            if (argsRoot is Vector3 position)
            {
                _SetPuppetPlayerPosition(position);
            }
        }

        public void Teleport(Vector3 position)
        {
            Debug.Assert(this.GetNetworkMode() == NetworkMode.Server);
            Player.SetGlobalPosition(position);
            RpcId(Player.State.OwnerPeerId, nameof(_Teleport), position);
        }

        [Master]
        private void _RequestMove(Vector3 target)
        {
            if (Player.Shared.ValidateOwnerOnlyRpc(nameof(_RequestMove))) return;
            Player.SetGlobalPosition(target);
            var ownerPeerId = GetTree().GetRpcSenderId();
            foreach (var viewer in this.EnumerateNetworkViewers())
            {
                if (viewer == ownerPeerId) continue;
                RpcUnreliableId(viewer, nameof(_SetPuppetPlayerPosition), target);
            }
        }
        
        [Puppet]
        private void _SetPuppetPlayerPosition(Vector3 position)
        {
            Player.SetGlobalPosition(position);
        }

        [Puppet]
        private void _Teleport(Vector3 position)  // TODO: Reject movement packets that don't confirm teleport.
        {
            Player.SetGlobalPosition(position);
        }

        public void ReplicateMyPosition()
        {
            this.RpcUnreliableMaster(nameof(_RequestMove), Player.GetGlobalPosition());
        }
    }
}