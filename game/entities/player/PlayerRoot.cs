using Godot;

namespace Overlords.game.entities.player
{
    public class PlayerRoot: Node
    {
        public PlayerState State => GetNode<PlayerState>("State");
    }
}