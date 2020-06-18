using Godot;
using Overlords.game.world.entityCore;
using Overlords.helpers.csharp;
using Overlords.helpers.network;

namespace Overlords.game.entities.player
{
    public class PlayerShared : Node, IQuarantinedListener
    {
        private PlayerRoot Root => GetNode<PlayerRoot>("../../");
        
        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Client) return;
            this.FlagQuarantineListener();
        }

        public void OnSetupComplete()
        {
            Root.AddToGroup(EntityTypes.PlayersGroupName);
            var position = Root.State.InitialPosition.Value;
            Root.SetGlobalPosition(position);
        }

        public void _QuarantineOver()
        {
            OnSetupComplete();
        }
    }
}