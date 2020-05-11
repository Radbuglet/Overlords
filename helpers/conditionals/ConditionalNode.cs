using System.Linq;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;

namespace Overlords.helpers.conditionals
{
    public abstract class ConditionalNode : Node
    {
        [Export] private readonly Array<NodePath> _parallelTargets = new Array<NodePath>();
        
        protected abstract bool ShouldExist();

        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            ConditionallyPurgeNow(true);
        }

        public void ConditionallyPurgeNow(bool removeSelf)
        {
            if (ShouldExist()) return;
            NodePurging.PurgeParallel(_parallelTargets.ConvertToNodeIterator(this));
            this.PurgeSelf(removeSelf);
        }
    }
}