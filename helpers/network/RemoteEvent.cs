using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.network
{
    public class RemoteEvent : Node, IGenericRpc<object>
    {
        [Signal]
        public delegate void FiredRemotely();

        public void GenericFire(IEnumerable<int> targets, bool reliable, object args)
        {
            this.RpcGeneric(targets, nameof(_FiredRemotelyIntl), reliable, args);
        }

        [Remote]
        private void _FiredRemotelyIntl(object data)
        {
            var tree = GetTree();
            var senderId = tree.GetRpcSenderId();
            if (tree.GetNetworkMode() == NetworkTypeUtils.NetworkMode.Client && senderId != GetNetworkMaster())
            {
                GD.PushWarning(
                    "Non-master attempted to call this remote event but clients only accept RemoteEvent calls from masters!");
                return;
            }

            EmitSignal(nameof(FiredRemotely), senderId, data);
        }
    }
}