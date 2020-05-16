using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerClient: Node
    {
        [LinkNodeStatic("../../RemoteEvents/LoggedIn")]
        public RemoteEvent RemoteLoggedIn;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            RemoteLoggedIn.Connect(nameof(RemoteEvent.FiredRemotely), this, nameof(_LoggedIn));
        }

        private void _LoggedIn(int sender, object arg)
        {
            GD.Print("Logged in!");
        }
    }
}