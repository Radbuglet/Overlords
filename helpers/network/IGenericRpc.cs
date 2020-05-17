using Godot;

namespace Overlords.helpers.network
{
    public interface IGenericRpc<in TData>
    {
        void GenericFire(int? target, bool reliable, TData data);
    }

    public static class GenericRpcAliases
    {
        public static void Fire<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(null, true, data);
        }
		
        public static void FireId<TSelf, TData>(this TSelf self, int target, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(target, true, data);
        }
        
        public static void FireUnreliable<TSelf, TData>(this TSelf self, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(null, false, data);
        }
        
        public static void FireUnreliableId<TSelf, TData>(this TSelf self, int target, TData data) where TSelf: Node, IGenericRpc<TData>
        {
            self.GenericFire(target, false, data);
        }
    }
}