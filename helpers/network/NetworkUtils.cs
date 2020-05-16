using Godot;

namespace Overlords.helpers.network
{
    public static class NetworkUtils
    {
        public static Error StartServer(this SceneTree tree, int port, int maxConnections)
        {
            var peer = new NetworkedMultiplayerENet();
            var err = peer.CreateServer(port, maxConnections);
            if (err != Error.Ok)
                return err;
            tree.NetworkPeer = peer;  // TODO: Disable server relay
            return Error.Ok;
        }
        
        public static Error StartClient(this SceneTree tree, string host, int port)
        {
            var peer = new NetworkedMultiplayerENet();
            var err = peer.CreateClient(host, port);
            if (err != Error.Ok)
                return err;
            tree.NetworkPeer = peer;
            return Error.Ok;
        }
        
        public enum NetworkMode
        {
            Server,
            Client,
            None
        }

        public static NetworkMode GetNetworkMode(this SceneTree tree)
        {
            if (!tree.HasNetworkPeer()) return NetworkMode.None;
            return tree.IsNetworkServer() ? NetworkMode.Server : NetworkMode.Client;
        }

        public static NetworkMode GetNetworkMode(this Node node)
        {
            return node.GetTree().GetNetworkMode();
        }

        public static void RpcGeneric(this Node from, string targetFunc, int? targetPeer, bool reliable, params object[] args)
        {
            if (targetPeer == null)
            {
                if (reliable)
                    from.Rpc(targetFunc, args);
                else
                    from.RpcUnreliable(targetFunc, args);
            }
            else
            {
                if (reliable)
                    from.RpcId(targetPeer.Value, targetFunc, args);
                else
                    from.RpcUnreliableId(targetPeer.Value, targetFunc, args);
            }
        }
    }
}