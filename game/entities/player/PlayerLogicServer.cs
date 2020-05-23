using Godot;
using Overlords.game.world;
using Overlords.helpers.csharp;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player
{
    public class PlayerLogicServer: Node
    {
        [RequireBehavior] public PlayerLogicShared LogicShared;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            LogicShared.BalanceValue.Value = (int) GD.RandRange(0, 128);
            LogicShared.HasCharacterValue.Value = true;
            LogicShared.BuildCharacter();
        }

        public override void _Process(float delta)
        {
            if (!(GD.RandRange(0, 100) > 99)) return;
            SetValueBroadcasted(LogicShared.BalanceValue, (int) GD.RandRange(0, 100), true);
        }

        public PlayerProtocol.NetworkConstructor MakeConstructor(int target)
        {
            return new PlayerProtocol.NetworkConstructor
            {
                OwnerPeerId = LogicShared.OwnerPeerId,
                ReplicatedState = LogicShared.StateReplicator.SerializeValues()
            };
        }

        public void SetValueBroadcasted<T>(StateField<T> field, T value, bool reliable)
        {
            field.Value = value;
            LogicShared.StateReplicator.ReplicateValues(
                LogicShared.WorldRoot.GetBehavior<WorldLogicShared>().GetPlayingPeers(),
                field.AsEnumerable(), reliable);
        }
    }
}