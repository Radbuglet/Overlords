using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player.movement
{
    public class PlayerMovementNet : Node, IRequiresCatchup, IQuarantineInfectable
    {
        public PlayerRoot Root => GetNode<PlayerRoot>("../../../");
        private bool _gotInitialPosition;
        
        public override void _Ready()
        {
            this.Initialize();
        }

        public void CatchupState(int peerId)
        {
            RpcId(peerId, nameof(_SetPlayerPosition), Root.GetGlobalPosition());
        }

        [Master]
        private void _RequestMove(Vector3 target)
        {
            if (Root.SharedLogic.ValidateOwnerOnlyRpc(nameof(_RequestMove))) return;
            Root.SetGlobalPosition(target);
            var ownerPeerId = GetTree().GetRpcSenderId();
            foreach (var peerId in GetTree().GetPlayingPeers())
            {
                if (peerId == ownerPeerId) continue;
                RpcUnreliableId(peerId, nameof(_SetPlayerPosition), target);
            }
        }
        
        [Puppet]
        private void _SetPlayerPosition(Vector3 position)
        {
            Root.SetGlobalPosition(position);
            _gotInitialPosition = true;
        }
        
        public void _QuarantineChecking()
        {
            if (!_gotInitialPosition)
                throw new QuarantineContamination("Player never got its initial position!");
        }

        public void ReplicateMyPosition()
        {
            this.RpcUnreliableServer(nameof(_RequestMove), Root.GetGlobalPosition());
        }
    }
}