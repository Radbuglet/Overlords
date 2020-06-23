using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Overlords.helpers.tree.trackingGroup
{
    public interface INodeGroupTypeless
    {
        void InternalRemoveFromGroup(object key);
    }
    
    public class NodeGroup<TKey, TEntityBase> : INodeGroupTypeless where TEntityBase : Node
    {
        private readonly Dictionary<TKey, TEntityBase> _members = new Dictionary<TKey, TEntityBase>();

        public void InternalRemoveFromGroup(object key)
        {
            _members.Remove((TKey) key);
        }

        public void AddToGroup(TKey key, TEntityBase entity)
        {
            Debug.Assert(!_members.ContainsKey(key), "NodeGroup already contains a member with that ID!");
            _members.Add(key, entity);
            entity.GetTrackedMember(true).RegisterNodeInGroup(this, key);
        }

        public void RemoveFromGroup(TKey key)
        {
            var gotMember = _members.TryGetValue(key, out var removedMember);
            Debug.Assert(gotMember, "NodeGroup doesn't contain a member with that ID!");
            _members.Remove(key);
            removedMember.GetTrackedMember(true).UnregisterNodeFromGroup(this);
        }

        public void RemoveFromGroup(TEntityBase groupMember)
        {
            var memberTracker = groupMember.GetTrackedMember(true);
            _members.Remove(memberTracker.GetNodeIdForGroup(this));
            memberTracker.UnregisterNodeFromGroup(this);
        }

        public T GetMemberOfGroup<T>(TKey key, T fallback) where T : TEntityBase
        {
            return _members.TryGetValue(key, out var memberRaw) && memberRaw is T memberCast ? memberCast : fallback;
        }

        public IEnumerable<KeyValuePair<TKey, TEntityBase>> IterateGroupEntries()
        {
            return _members;
        }

        public IEnumerable<TKey> IterateGroupKeys()
        {
            return _members.Keys;
        }

        public IEnumerable<TEntityBase> IterateGroupMembers()
        {
            return _members.Values;
        }
    }

    public static class NodeGroupEntityExtensions
    {
        public static TrackedMember GetTrackedMember(this Node entityRoot, bool required)
        {
            var node = entityRoot.GetNodeOrNull<TrackedMember>(TrackedMember.ExpectedName);
            Debug.Assert(!required || node != null, $"Failed to get {nameof(TrackedMember)} handle in entity even though it was required!");
            return node;
        }
        
        public static bool GetIdInGroup<TKey, TBaseEnt>(this Node entity, NodeGroup<TKey, TBaseEnt> nodeGroup, out TKey key)
            where TBaseEnt : Node
        {
            var groupTracker = entity.GetTrackedMember(false);
            if (groupTracker == null)
            {
                key = default;
                return false;
            }
            key = groupTracker.GetNodeIdForGroup(nodeGroup);
            return true;
        }
    }
}