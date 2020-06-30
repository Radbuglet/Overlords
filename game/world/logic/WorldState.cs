using Overlords.game.entities.player;
using Overlords.game.world.entityCore;

namespace Overlords.game.world.logic
{
    public class WorldState : StateReplicator
    {
        public readonly ReplicatedField<int?> OverlordId;
        private WorldRoot WorldRoot => GetNode<WorldRoot>("../../");
        
        public WorldState()
        {
            OverlordId = AddField<int?>(isNullable: true);
        }

        public PlayerRoot GetOverlord()
        {
            var id = OverlordId.Value;
            return id == null ? null : WorldRoot.Shared.GetPlayer(id.Value);
        }
    }
}