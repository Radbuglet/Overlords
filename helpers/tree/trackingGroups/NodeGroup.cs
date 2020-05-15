using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.helpers.tree.trackingGroups
{
    public interface INodeGroupTypeless
    {
        void InternalRemoveFromGroup(object key);
    }
    
    public class NodeGroup<TKey, TEntityBase>: INodeGroupTypeless where TEntityBase: Node
    {
        private readonly Dictionary<TKey, TEntityBase> _members = new Dictionary<TKey, TEntityBase>();
        
        public void AddToGroup(TKey key, TEntityBase entity)
        {
            Debug.Assert(!_members.ContainsKey(key), "NodeGroup already contains a member with that ID!");
            _members.Add(key, entity);
            entity.GetBehavior<NodeGroupMemberTracker>().RegisterNodeInGroup(this, key);
        }

        public void RemoveFromGroup(TKey key)
        {
            var gotMember = _members.TryGetValue(key, out var removedMember);
            Debug.Assert(gotMember, "NodeGroup doesn't contain a member with that ID!");
            _members.Remove(key);
            removedMember.GetBehavior<NodeGroupMemberTracker>()
                .UnregisterNodeFromGroup(this);

        }
        
        public void RemoveFromGroup(TEntityBase groupMember)
        {
            var memberTracker = groupMember.GetBehavior<NodeGroupMemberTracker>();
            _members.Remove((TKey) memberTracker.GetNodeIdForGroup(this));
            memberTracker.UnregisterNodeFromGroup(this);
        }

        public T GetMemberOfGroup<T>(TKey key, T fallback) where T: TEntityBase
        {
            return _members.TryGetValue(key, out var memberRaw) && memberRaw is T memberCast ?
                memberCast : fallback;
        }

        public void InternalRemoveFromGroup(object key)
        {
            _members.Remove((TKey) key);
        }

        public IEnumerable<KeyValuePair<TKey, TEntityBase>> IterateGroupMembersEntries()
        {
            return _members;
        }
        
        public IEnumerable<TEntityBase> IterateGroupMembers()
        {
            return _members.Values;
        }
    }
}