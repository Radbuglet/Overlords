using Godot;

namespace Overlords.helpers.tree.conditionals
{
    public abstract class ConditionalNode : Node
    {
        protected abstract bool ShouldExist();

        public override void _EnterTree()
        {
            if (Engine.EditorHint) return;
            ConditionallyPurgeNow(true);
        }

        public void ConditionallyPurgeNow(bool removeSelf)
        {
            if (ShouldExist()) return;
            this.PurgeWhileLocked(removeSelf);
        }
    }
}