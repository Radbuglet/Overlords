using Godot;

namespace Overlords.helpers.network
{
	public class RemoteEvent: Node
	{
		[Signal]
		public delegate void FiredRemotely();  // Signature: int sender, object data

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
		
		public void Fire(int target, object data)
		{
			RpcId(target, nameof(HandleRemote), data);
		}
		
		public void FireUnreliable(object data)
		{
			RpcUnreliable(nameof(HandleRemote), data);
		}
		
		public void FireUnreliable(int target, object data)
		{
			RpcUnreliableId(target, nameof(HandleRemote), data);
		}

		public void GenericFire(int? target, bool reliable, object data)
		{
			if (target == null)
			{
				if (reliable)
					Fire(data);
				else
					FireUnreliable(data);
			}
			else
			{
				if (reliable)
					Fire(target.Value, data);
				else
					FireUnreliable(target.Value, data);
			}
		}
	}
}
