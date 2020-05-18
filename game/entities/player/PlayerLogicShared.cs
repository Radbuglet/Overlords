using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicShared: Node
    {
        [RequireParent] public Spatial PlayerRoot;
        
        public void InitializeShared(int peerId)
        {
            this.InitializeBehavior();
            PlayerRoot.Name = $"player_{peerId}";
        }
    }
}