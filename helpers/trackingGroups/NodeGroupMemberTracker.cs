using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.behaviors;

namespace Overlords.helpers.trackingGroups
{
    public class NodeGroupMemberTracker: Node
    {
        private readonly Dictionary<INodeGroupTypeless, object> _containedInGroups
            = new Dictionary<INodeGroupTypeless, object>();
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public override void _ExitTree()
        {
            if (Engine.EditorHint) return;
            foreach (var pair in _containedInGroups)
            {
                pair.Key.InternalRemoveFromGroup(pair.Value);
            }
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
    }
}