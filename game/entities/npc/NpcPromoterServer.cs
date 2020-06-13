using Godot;
using Overlords.game.definitions;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.npc
{
    public class NpcPromoterServer: Node
    {
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        [BindEntitySignal(nameof(GameSignals.OnEntityInteracted))]
        private void _InteractedWith(string myTargetId, Node playerRoot)
        {
            GD.Print("Promoting...");
            // TODO: Promote player if conditions are right
        }
    }
}