using Godot;

namespace Overlords.helpers.network
{
	public class RemoteEvent: Node
	{
		[Signal]
		public delegate void FiredRemotely();

		[Master]
		[Puppet]
		public void HandleRemote(object data)
		{
			EmitSignal(nameof(FiredRemotely), GetTree().GetRpcSenderId(), data);
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
	}
}
