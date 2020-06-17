using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.network
{
    public enum NetworkMode
    {
        Server,
        Client,
        None
    }
    
    public static class NetworkTypeUtils
    {
        [Flags]
        public enum ObjectVariant
        {
            FlagAuthoritative = 0b_0000_0001,
            FlagNotAuthoritative = 0b_0000_0010,
            FlagServer = 0b_0000_0100,
            FlagClient = 0b_0000_1000,
            Server = FlagAuthoritative | FlagServer,
            LocalAuthoritative = FlagAuthoritative | FlagClient,
            LocalPuppet = FlagNotAuthoritative | FlagClient
        }

        public static Error StartServer(this SceneTree tree, int port, int maxConnections)
        {
            var peer = new NetworkedMultiplayerENet {ServerRelay = false};
            var err = peer.CreateServer(port, maxConnections);
            if (err != Error.Ok)
                return err;
            tree.NetworkPeer = peer;
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

        public static NetworkMode GetNetworkMode(this SceneTree tree)
        {
            if (!tree.HasNetworkPeer()) return NetworkMode.None;
            return tree.IsNetworkServer() ? NetworkMode.Server : NetworkMode.Client;
        }

        public static NetworkMode GetNetworkMode(this Node node)
        {
            return node.GetTree().GetNetworkMode();
        }

        public static ObjectVariant GetNetworkVariant(this SceneTree tree, int ownerPeerId)
        {
            return tree.GetNetworkMode() == NetworkMode.Server
                ? ObjectVariant.Server
                : ownerPeerId == tree.GetNetworkUniqueId()
                    ? ObjectVariant.LocalAuthoritative
                    : ObjectVariant.LocalPuppet;
        }

        public static void ApplyToTree(this ObjectVariant variant, Dictionary<ObjectVariant, IEnumerable<Func<Node>>> config)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var group in config)
            {
                if ((variant & group.Key) == group.Key)  // True if all bits with values requested match up
                    continue;

                foreach (var node in group.Value)
                    node().Purge();
            }
        }

        public static void ApplyNetworkVariant(this Node gameObject, ObjectVariant variant, Type behaviorServer,
            Type behaviorClientShared, Type behaviorClientLocal, Type behaviorClientPuppet)
        {
            void RemoveBehavior(Type type)
            {
                if (type == null) return;
                gameObject.GetBehaviorDynamic(type).Purge();
            }

            if (variant == ObjectVariant.Server)
            {
                RemoveBehavior(behaviorClientShared);
                RemoveBehavior(behaviorClientLocal);
                RemoveBehavior(behaviorClientPuppet);
            }
            else
            {
                RemoveBehavior(behaviorServer);
                RemoveBehavior(variant == ObjectVariant.LocalAuthoritative
                    ? behaviorClientPuppet
                    : behaviorClientLocal);
            }
        }
    }
}