using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.common;
using Overlords.game.entities.player;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.npcs.promoter
{
    public class PromoterServer : Node, IWorldProvider
    {
        public Node World => this.GetWorldFromEntity();
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        [BindEntitySignal(nameof(GameSignals.OnEntityInteracted))]
        private void _OnInteracted(string myTargetId, Node playerRoot)
        {
            this.GetWorldServer().ChangeOverlord(playerRoot.GetBehavior<PlayerShared>().OwnerPeerId);
        }
    }
}