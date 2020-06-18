using Godot;

namespace Overlords.helpers.network
{
    public static class RpcUtils
    {
        public static void RpcServer(this Node node, string method, params object[] args)
        {
            node.RpcId(node.GetNetworkMaster(), method, args);
        }
        
        public static void RpcUnreliableServer(this Node node, string method, params object[] args)
        {
            node.RpcUnreliableId(node.GetNetworkMaster(), method, args);
        }
    }
}