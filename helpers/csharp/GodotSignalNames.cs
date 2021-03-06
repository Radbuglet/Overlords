namespace Overlords.helpers.csharp
{
    public static class SceneTreeSignals
    {
        public const string ConnectedToServer = "connected_to_server";
        public const string ConnectionFailed = "connection_failed";
        public const string NetworkPeerConnected = "network_peer_connected";
        public const string NetworkPeerDisconnected = "network_peer_disconnected";
        public const string ServerDisconnected = "server_disconnected";
        public const string NodeAdded = "node_added";
    }

    public static class NodeSignals
    {
        public const string TreeExited = "tree_exited";
    }
}