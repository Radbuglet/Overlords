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

        public PlayerProtocol.NetworkConstructor MakeConstructor(int target)
        {
            return new PlayerProtocol.NetworkConstructor
            {
                OwnerPeerId = LogicShared.OwnerPeerId,
                State = new PlayerProtocol.PlayerInitialState
                {
                    Balance = LogicShared.Balance,
                    CharacterState = LogicShared.HasCharacter() ? new PlayerProtocol.CharacterInitialState
                    {
                        Position = LogicShared.CharacterRoot.Translation
                    } : null
                }
            };
        }
    }
}