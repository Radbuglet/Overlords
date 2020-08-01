using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.tree;

namespace Overlords.helpers.network
{
    public enum NetworkMode
    {
        Server,
        Client,
        None
    }
    
    [Flags]
    public enum NetObjectVariant
    {
        FlagAuthoritative = 0b_0000_0001,
        FlagNotAuthoritative = 0b_0000_0010,
        FlagServer = 0b_0000_0100,
        FlagClient = 0b_0000_1000,
        Server = FlagAuthoritative | FlagServer,
        LocalAuthoritative = FlagAuthoritative | FlagClient,
        LocalPuppet = FlagNotAuthoritative | FlagClient
    }
    
    public static class NetworkTypeUtils
    {
        /// <summary>
        /// Creates a server without all that peer to peer nonsense.
        /// Will silently replace any pre-existing peer without performing any shutdown/cleanup logic.
        /// </summary>
        /// <returns>
        /// The return value from CreateServer().
        /// `Error.Ok` signifies that the server was created and bounded.
        /// </returns>
        public static Error StartServer(this SceneTree tree, int port, int maxConnections)
        {
            var peer = new NetworkedMultiplayerENet {ServerRelay = false};
            var err = peer.CreateServer(port, maxConnections);
            if (err != Error.Ok)
                return err;
            tree.NetworkPeer = peer;
            return Error.Ok;
        }

        /// <summary>
        /// Creates a client.
        /// Will silently replace any pre-existing peer without performing any shutdown/cleanup logic.
        /// </summary>
        /// <returns>
        /// The return value from CreateClient().
        /// `Error.Ok` signifies that the client was created and bounded.
        /// </returns>
        public static Error StartClient(this SceneTree tree, string host, int port)
        {
            var peer = new NetworkedMultiplayerENet();
            var err = peer.CreateClient(host, port);
            if (err != Error.Ok)
                return err;
            tree.NetworkPeer = peer;
            return Error.Ok;
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

        public static NetObjectVariant GetNetworkVariant(this SceneTree tree, int ownerPeerId)
        {
            return tree.GetNetworkMode() == NetworkMode.Server
                ? NetObjectVariant.Server
                : ownerPeerId == tree.GetNetworkUniqueId()
                    ? NetObjectVariant.LocalAuthoritative
                    : NetObjectVariant.LocalPuppet;
        }

        /// <summary>
        /// Removes all nodes that fall under sections of the config whose keys do not match with the provided variant.
        /// The config keys are specified as bit masks of NetObjectVariant. All non-zero bits from any given key must match
        /// the provided variant in order for it to be considered matched.
        /// </summary>
        public static void ApplyToTree(this NetObjectVariant variant, Dictionary<NetObjectVariant, IEnumerable<Func<Node>>> config)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var group in config)
            {
                if ((variant & group.Key) == group.Key)  // True if all bits with values requested match up
                    continue;  // Do not delete the nodes in this group.

                foreach (var node in group.Value)
                    node().Purge();
            }
        }
        
        public static void RpcMaster(this Node node, string methodName, params object[] args)
        {
            node.RpcId(node.GetNetworkMaster(), methodName, args);
        }
        
        public static void RpcUnreliableMaster(this Node node, string methodName, params object[] args)
        {
            node.RpcUnreliableId(node.GetNetworkMaster(), methodName, args);
        }
    }
}