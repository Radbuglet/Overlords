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
                _SetPlayerPosition(position);
            }
        }

        [Master]
        private void _RequestMove(Vector3 target)
        {
            if (Player.Shared.ValidateOwnerOnlyRpc(nameof(_RequestMove))) return;
            Player.SetGlobalPosition(target);
            var ownerPeerId = GetTree().GetRpcSenderId();
            this.RpcUnreliableVis(nameof(_SetPlayerPosition), target);
        }
        
        [Puppet]
        private void _SetPlayerPosition(Vector3 position)
        {
            Player.SetGlobalPosition(position);
        }

        public void ReplicateMyPosition()
        {
            this.Rpc(nameof(_RequestMove), Player.GetGlobalPosition());
        }
    }
}