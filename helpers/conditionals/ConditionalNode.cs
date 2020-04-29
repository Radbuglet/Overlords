using Godot;

namespace Overlords.helpers.conditionals
{
    public abstract class ConditionalNode : Node
    {
        protected abstract bool ShouldExist();

        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            if (ShouldExist()) return;

            foreach (var child in GetChildren())
            {
                RemoveChild((Node) child);
            }
            QueueFree();
        }
    }
}