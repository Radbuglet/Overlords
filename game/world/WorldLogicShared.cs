using Godot;
using Godot.Collections;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicShared: Node
    {
        [FieldNotNull] [Export]
        public Array<PackedScene> EntityTypes;
        
        public readonly NodeGroup<int, Node> GroupPlayers = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}