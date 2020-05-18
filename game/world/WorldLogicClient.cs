using Godot;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldLogicClient: Node
    {
        [LinkNodeStatic("../EntityContainer")]
        public ListReplicator EntityContainer;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityContainer.BuildRemoteInstance = instance =>
            {
                
            };
        }
    }
}