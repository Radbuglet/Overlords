using Godot;
using Overlords.game.world.entityCore;
using Overlords.helpers.tree;

namespace Overlords.game.world.logic
{
    public class WorldRoot : Node
    {
        [LinkNodeStatic("Entities")] public EntityContainer Entities;
        
        public override void _EnterTree()
        {
            this.Initialize();
        }
    }
}