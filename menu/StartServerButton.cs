using Godot;
using Overlords.helpers.network;

namespace Overlords.menu
{
    public class StartServerButton: Button
    {
        public override void _Pressed()
        {
            var result = GetTree().StartServer(8080, 32);
            if (result != Error.Ok)
            {
                OS.Alert($"Failed to create server! Error code: Error.{result}");
                return;
            }
            GetTree().ChangeScene("res://game/TestWorld.tscn");
        }
    }
}