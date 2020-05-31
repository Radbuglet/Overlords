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
        // Primitives
        public static void Fire<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(null, true, data);
        }
		
        public static void FireId<TSelf, TData>(this TSelf self, IEnumerable<int> targets, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(targets, true, data);
        }

        public static void FireUnreliable<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(null, false, data);
        }
        
        public static void FireUnreliableId<TSelf, TData>(this TSelf self, IEnumerable<int> targets, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(targets, false, data);
        }

        // Single ID
        public static void FireId<TSelf, TData>(this TSelf self, int target, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(target.AsEnumerable(), true, data);
        }
        
        public static void FireUnreliableId<TSelf, TData>(this TSelf self, int target, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(target.AsEnumerable(), false, data);
        }
        
        // Server
        public static void FireServer<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            Debug.Assert(self.GetNetworkMode() == NetworkUtils.NetworkMode.Client);
            self.FireId(1, data);
        }
        
        public static void FireUnreliableServer<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            Debug.Assert(self.GetNetworkMode() == NetworkUtils.NetworkMode.Client);
            self.FireUnreliableId(1, data);
        }
    }
}