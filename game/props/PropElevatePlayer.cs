using Godot;
using Overlords.game.player;
using Overlords.helpers.csharp;

namespace Overlords.game.props
{
    public class PropElevatePlayer : Spatial, IProp
    {
        public void _OnObjectInteracted(PlayerRoot player)
        {
            var state = this.GetScene<GameRoot>().State;
            state.SvOverlordChanged(player.State.OwnerPeerId);
            GD.Print("New overlord!");
        }
    }
}