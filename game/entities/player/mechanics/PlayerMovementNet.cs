using Godot;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.entities.player.mechanics
{
    public class PlayerMovementNet : Node, ICatchesUpSelf, IInvariantEnforcer
    {
        public PlayerRoot Root => GetNode<PlayerRoot>("../../../");
        public bool DeniesCatchup { get; set; }

        public CatchupState CatchupOverNetwork(int peerId)
        {
            return new CatchupState(true, Root.GetGlobalPosition());
        }
        
        public void HandleCatchupState(object argsRoot)
        {
            if (argsRoot is Vector3 position)
            {
                _SetPlayerPosition(position);
            }
        }
        
        public void ValidateCatchupState(SceneTree tree)
        {
            if (!DeniesCatchup)
                throw new InvalidCatchupException("Never received initial position.");
        }

        [Master]
        private void _RequestMove(Vector3 target)
        {
            if (Root.SharedLogic.ValidateOwnerOnlyRpc(nameof(_RequestMove))) return;
            Root.SetGlobalPosition(target);
            var ownerPeerId = GetTree().GetRpcSenderId();
            foreach (var peerId in this.GetWorldRoot().Shared.GetOnlinePeers())
            {
                if (peerId == ownerPeerId) continue;
                RpcUnreliableId(peerId, nameof(_SetPlayerPosition), target);
            }
        }
        
        [Puppet]
        private void _SetPlayerPosition(Vector3 position)
        {
            Root.SetGlobalPosition(position);
        }

        public void ReplicateMyPosition()
        {
            this.RpcUnreliableServer(nameof(_RequestMove), Root.GetGlobalPosition());
        }
    }
}