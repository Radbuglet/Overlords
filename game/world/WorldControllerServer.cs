using Godot;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerServer: Node
    {
        [Export] private NodePath _pathToEntities;

        [LinkNodePath(nameof(_pathToEntities))]
        public ListReplicator EntityReplicator;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}