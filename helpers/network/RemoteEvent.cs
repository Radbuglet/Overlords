using Godot;

namespace Overlords.helpers.network
{
	public class RemoteEvent: Node
	{
		[Signal]
		public delegate void FiredRemotely();

		[Remote]
		public void HandleRemote(object data)
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

		public void Fire(object data)
		{
			Rpc(nameof(HandleRemote), data);
		}
		
		public void FireId(int target, object data)
		{
			RpcId(target, nameof(HandleRemote), data);
		}
		
		public void FireUnreliable(object data)
		{
			RpcUnreliable(nameof(HandleRemote), data);
		}
		
		public void FireUnreliableId(int target, object data)
		{
			RpcUnreliableId(target, nameof(HandleRemote), data);
		}

		public void GenericFire(int? target, bool reliable, object data)
		{
			this.RpcGeneric(nameof(HandleRemote), target, reliable, data);
		}
	}
}
