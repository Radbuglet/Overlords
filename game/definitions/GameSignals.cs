using Godot;

namespace Overlords.game.definitions
{
    public static class GameSignals
    {
        public delegate void OnEntityInteracted(string myTargetId, Node characterRoot);
    }
}