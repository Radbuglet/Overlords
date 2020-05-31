using System.Collections.Generic;
using Godot;

namespace Overlords.helpers.network
{
	public class RemoteEvent: Node, IGenericRpc<object>
	{
		[Signal]
		public delegate void FiredRemotely();

		[Remote]
		private void HandleRemote(object data)
		{
			var tree = GetTree();
			var senderId = tree.GetRpcSenderId();
			if (tree.GetNetworkMode() == NetworkUtils.NetworkMode.Client && senderId != GetNetworkMaster())
			{
				GD.PushWarning("Non-master attempted to call this remote event but clients only accept RemoteEvent calls from masters!");
				return;
			}
			EmitSignal(nameof(FiredRemotely), senderId, data);
		}

		public void GenericFire(IEnumerable<int> targets, bool reliable, object args)
		{
			if (targets == null)
			{
				if (reliable)
					Rpc(nameof(HandleRemote), args);
				else
					RpcUnreliable(nameof(HandleRemote), args);
			}
			else
			{
				if (reliable)
				{
					foreach (var target in targets)
					{
						RpcId(target, nameof(HandleRemote), args);
					}
				}
				else
				{
					foreach (var target in targets)
					{
						RpcUnreliableId(target, nameof(HandleRemote), args);
					}
				}
			}
		}
	}
}
