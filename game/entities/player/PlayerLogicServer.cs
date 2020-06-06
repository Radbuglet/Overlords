using System.Collections.Generic;
using Godot;
using Overlords.game.entities.player.character;
using Overlords.game.world;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicServer : Node
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
                State = new PlayerProtocol.InitialState
                {
                    CharacterState = LogicShared.HasCharacter()
                        ? LogicShared.CharacterRoot.GetBehavior<CharacterLogicServer>().MakeConstructor(target)
                        : null
                },
                ReplicatedValues = LogicShared.StateReplicator.SerializeValuesCatchup()
            };
        }

        private IEnumerable<int> GetPlayingPeers()
        {
            return LogicShared.WorldRoot.GetBehavior<WorldLogicShared>().GetPlayingPeers();
        }

        public void SetBalanceReplicated(int newBalance)
        {
            GD.Print($"Setting balance from {LogicShared.Balance.Value} to {newBalance}...");
            LogicShared.StateReplicator.SetValueReplicated(GetPlayingPeers(), LogicShared.Balance, newBalance, true);
        }
    }
}