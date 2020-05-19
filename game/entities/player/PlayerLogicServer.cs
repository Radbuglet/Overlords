using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicServer: Node
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public object SerializeConstructor(int target)
        {
            return new PlayerLogicShared.NetworkConstructor
            {
                OwnerPeerId = LogicShared.OwnerPeerId
            }.Serialize();
        }
    }
}