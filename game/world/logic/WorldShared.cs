using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.helpers.tree.trackingGroup;

namespace Overlords.game.world.logic
{
    public class WorldShared : Node
    {
        public readonly NodeGroup<string, Node> InteractionTargets = new NodeGroup<string, Node>();
        private WorldRoot WorldRoot => GetNode<WorldRoot>("../../");

        public override void _Ready()
        {
            foreach (var node in GetTree().GetNodesInGroup(EntityTypes.RegisterInteractionGroupName).Cast<Node>())
            {
                InteractionTargets.AddToGroup(node.Name, node);
            }
        }
    }
}