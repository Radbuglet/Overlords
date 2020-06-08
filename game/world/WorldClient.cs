using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldClient : Node
    {
        [RequireBehavior] public WorldShared LogicShared;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }
    }
}