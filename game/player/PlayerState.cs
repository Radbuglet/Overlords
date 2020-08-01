using Overlords.helpers.replication;

namespace Overlords.game.player
{
    public class PlayerState : StateConstructor
    {
        public int OwnerPeerId;
        public string DisplayName;
        public int Balance;
        public int Health;
        
        public PlayerState()
        {
            AddField(() => OwnerPeerId, v => OwnerPeerId = v);
            AddField(() => DisplayName, v => DisplayName = v);
            AddField(() => Balance, v => Balance = v);
            AddField(() => Health, v => Health = v);
        }
    }
}