using Godot;
using Godot.Collections;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicShared: Node
    {
        [Export]
        private Array<PackedScene> _editorEntityTypes = new Array<PackedScene>();
        
        public readonly EntityTypeRegistrar TypeRegistrar = new EntityTypeRegistrar();
        public readonly NodeGroup<int, Node> GroupPlayers = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            TypeRegistrar.RegisterTypes(_editorEntityTypes);
        }
    }
}