using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerMovementNet : Node, IRequiresCatchup, IInvariantEnforcer
    {
        public PlayerRoot Root => GetNode<PlayerRoot>("../../../");
        private bool _gotInitialPosition;

        public CatchupState CatchupOverNetwork(int peerId)
        {
            return new CatchupState(Root.GetGlobalPosition(), true);
        }
        
        public void HandleCatchupState(object remoteArgs)
        {
            if (remoteArgs is Vector3 position)
            {
                _SetPlayerPosition(position);
            }
        }
        
        public void ValidateCatchupState(SceneTree tree)
        {
            if (!_gotInitialPosition)
                throw new InvalidCatchupException("Never received initial position.");
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

        public void ReplicateMyPosition()
        {
            this.RpcUnreliableServer(nameof(_RequestMove), Root.GetGlobalPosition());
        }
    }
}