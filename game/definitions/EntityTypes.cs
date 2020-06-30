using Godot;
using Overlords.game.entities.player;
using Overlords.game.world.logic;
using Overlords.helpers.csharp;

namespace Overlords.game.definitions
{
    public static class EntityTypes
    {
        public delegate void SignalOnInteracted(PlayerRoot player);
        public const string RegisterInteractionGroupName = "auto_interactable";

        public static WorldRoot GetWorldRoot(this Node node)
        {
            return node.GetScene<WorldRoot>();
        }
    }
}