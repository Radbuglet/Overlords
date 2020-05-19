using Godot;
using Overlords.helpers;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.services
{
    public interface IParentEnterTrigger
    {
        void _EarlyEditorTrigger(SceneTree tree);
    }
    
    public class EarlyTriggerService: Node
    {
        public override void _Ready()
        {
            this.InitializeBehavior();
            GetTree().Connect(SceneTreeSignals.NodeAdded, this, nameof(_NodeAdded));
        }

        private void _NodeAdded(Node instance)
        {
            var tree = GetTree();
            foreach (var child in instance.EnumerateChildren())
            {
                (child as IParentEnterTrigger)?._EarlyEditorTrigger(tree);
            }
        }
    }
}