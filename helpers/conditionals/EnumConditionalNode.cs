using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace Overlords.helpers.conditionals
{
    public class EnumConditionalNode: Node
    {
        [Export] private readonly Array<NodePath> _allNodes = new Array<NodePath>();  // TODO: Configuring is a bit jank. Make a better system once the inspector is fixed.
        [Export] private readonly Array<Array<NodePath>> _conditions = new Array<Array<NodePath>>();
        [Export] public int ActiveCaseIndex;
        
        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            ConditionallyPurgeNow();
        }

        public void ConditionallyPurgeNow()
        {
            Debug.Assert(ActiveCaseIndex > 0 && ActiveCaseIndex < _conditions.Count, "Invalid activeCaseIndex for EnumConditionalNode.");
            var activeCondition = _conditions[ActiveCaseIndex];
            foreach (var nodePath in _allNodes)
            {
                if (activeCondition.Contains(nodePath)) continue;
                var target = GetNodeOrNull(nodePath);
                if (target == null) continue;
                NodePurging.PurgeParallel(target);
            }
        }
    }
}