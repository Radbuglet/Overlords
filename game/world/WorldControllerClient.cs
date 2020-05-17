using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerClient: Node
    {
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        private void _PlayerBalanceChanged(int newBalance, Node player)
        {
            
        }
    }
}