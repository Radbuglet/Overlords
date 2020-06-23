using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Overlords.helpers.tree.trackingGroup
{
    public class TrackedMember : Node
    {
        public const string ExpectedName = nameof(TrackedMember);
        
        private readonly Dictionary<INodeGroupTypeless, object> _containedInGroups
            = new Dictionary<INodeGroupTypeless, object>();

        public override void _Ready()
        {
            Debug.Assert(Name == ExpectedName, $"All {nameof(TrackedMember)}s must be named \"{ExpectedName}\"");
        }

        public override void _ExitTree()
        {
            if (Engine.EditorHint) return;
            foreach (var pair in _containedInGroups) pair.Key.InternalRemoveFromGroup(pair.Value);
        }

        public void RegisterNodeInGroup(INodeGroupTypeless nodeGroup, object id)
        {
            Debug.Assert(!_containedInGroups.ContainsKey(nodeGroup), "Node already contained in the group.");
            _containedInGroups.Add(nodeGroup, id);
        }

        public void UnregisterNodeFromGroup(INodeGroupTypeless nodeGroup)
        {
            Debug.Assert(_containedInGroups.ContainsKey(nodeGroup), "Node was never in the group to begin with.");
            _containedInGroups.Remove(nodeGroup);
        }

        public object GetNodeIdForGroup(INodeGroupTypeless nodeGroup)
        {
            var foundGroupId = _containedInGroups.TryGetValue(nodeGroup, out var id);
            Debug.Assert(foundGroupId, "Failed to get group member id.");
            return id;
        }
        
        public TKey GetNodeIdForGroup<TKey, TEntityBase>(NodeGroup<TKey, TEntityBase> nodeGroup) where TEntityBase : Node
        {
            var foundGroupId = _containedInGroups.TryGetValue(nodeGroup, out var id);
            Debug.Assert(foundGroupId, "Failed to get group member id.");
            return (TKey) id;
        }
		
        public TKey GetNodeIdForGroup<TKey, TBaseEnt>(NodeGroup<TKey, TBaseEnt> @group, TKey fallback) where TBaseEnt: Node
        {
            return _containedInGroups.TryGetValue(@group, out var id) ? (TKey) id : fallback;
        }
    }
}