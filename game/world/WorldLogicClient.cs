using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldLogicClient : Node
    {
        [RequireBehavior] public WorldLogicShared LogicShared;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}