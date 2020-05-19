using Godot;
using Overlords.game.world;
using Overlords.helpers.csharp;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicServer: Node, IEntityCatchupEmitter
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            this.DeclareImplementation(typeof(IEntityCatchupEmitter).AsEnumerable());
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