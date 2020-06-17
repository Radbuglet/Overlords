using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.csharp;

namespace Overlords.helpers.network
{
    public interface IGenericRpc<in TData>
    {
        void GenericFire(IEnumerable<int> targets, bool reliable, TData data);
    }

    public static class GenericRpcAliases
    {
        // Extension to RPC
        public static void RpcGeneric(this Node self, IEnumerable<int> targets, string methodName, bool reliable,
            object args)
        {
            if (targets == null)
            {
                if (reliable)
                    self.Rpc(methodName, args);
                else
                    self.RpcUnreliable(methodName, args);
            }
            else if (reliable)
            {
                foreach (var target in targets) self.RpcId(target, methodName, args);
            }
            else
            {
                foreach (var target in targets) self.RpcUnreliableId(target, methodName, args);
            }
        }

        // Primitives
        public static void Fire<TSelf, TData>(this TSelf self, TData data) where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(null, true, data);
        }

        public static void FireId<TSelf, TData>(this TSelf self, IEnumerable<int> targets, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(targets, true, data);
        }

        public static void FireUnreliable<TSelf, TData>(this TSelf self, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(null, false, data);
        }

        public static void FireUnreliableId<TSelf, TData>(this TSelf self, IEnumerable<int> targets, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(targets, false, data);
        }

        // Single ID
        public static void FireId<TSelf, TData>(this TSelf self, int target, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(target.AsEnumerable(), true, data);
        }

        public static void FireUnreliableId<TSelf, TData>(this TSelf self, int target, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            self.GenericFire(target.AsEnumerable(), false, data);
        }

        // Server
        public static void FireServer<TSelf, TData>(this TSelf self, TData data) where TSelf : Node, IGenericRpc<TData>
        {
            Debug.Assert(self.GetNetworkMode() == NetworkMode.Client);
            self.FireId(1, data);
        }

        public static void FireUnreliableServer<TSelf, TData>(this TSelf self, TData data)
            where TSelf : Node, IGenericRpc<TData>
        {
            Debug.Assert(self.GetNetworkMode() == NetworkMode.Client);
            self.FireUnreliableId(1, data);
        }
    }
}