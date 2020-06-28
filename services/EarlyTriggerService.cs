using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;

namespace Overlords.services
{
    public interface IParentEnterTrigger
    {
        void _EarlyEditorTrigger(SceneTree tree);
    }

    /// <summary>
    /// A service which allows nodes to run logic while being added to the SceneTree before they are locked.
    /// This works by taking any node that has just entered the tree and notifying its children, who are not yet locked.
    /// </summary>
    public class EarlyTriggerService : Node
    {
        public override void _Ready()
        {
            this.Initialize();
            GetTree().Connect(SceneTreeSignals.NodeAdded, this, nameof(_NodeAdded));
        }

        private void _NodeAdded(Node instance)
        {
            var tree = GetTree();
            foreach (var child in instance.EnumerateChildren())
                (child as IParentEnterTrigger)?._EarlyEditorTrigger(tree);
        }
    }
}