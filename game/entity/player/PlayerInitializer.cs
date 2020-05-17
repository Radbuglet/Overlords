using Godot;
using Overlords.game.world;
using Overlords.helpers.tree;

namespace Overlords.game.entity.player
{
    public class PlayerInitializer: Node
    {
        public override void _Ready()
        {
            GD.PushWarning($"{nameof(PlayerInitializer)} persisted into the SceneTree!");
        }

        public void Initialize(Protocol.PlayerConstructor constructor)
        {
            this.Purge();
        }
    }
}