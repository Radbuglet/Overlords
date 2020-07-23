using Godot;
using Overlords.helpers.network;

namespace Overlords.menu
{
    public class StartClientButton : Button
    {
        public override void _Pressed()
        {
            var result = GetTree().StartClient("localhost", 8080);
            if (result != Error.Ok)
            {
                OS.Alert($"Failed to start connection with server. Error code: Error.{result}");
                return;
            }

            GetTree().ChangeScene("res://game/TestWorld.tscn");
        }
    }
}