using Godot;
using Godot.Collections;

namespace Overlords.helpers.conditionals
{
    public abstract class ConditionalNode : Node
    {
        [Export] private readonly Array<NodePath> _parallelNodes = new Array<NodePath>();
        
        protected abstract bool ShouldExist();

        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            if (ShouldExist()) return;

            foreach (var child in GetChildren())
            {
                RemoveChild((Node) child);  // TODO: What about freeing?
            }
            
            foreach (var parallelNp in _parallelNodes)
            {
                var parallelNode = GetNode(parallelNp);
                parallelNode.GetParent().RemoveChild(parallelNode);
            }
            QueueFree();
        }
    }
}