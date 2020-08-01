using System;
using Overlords.helpers.replication;

namespace Overlords.game
{
    public class GameState : StateConstructor
    {
        public int? OverlordId;
        
        public GameState()
        {
            AddField(() => OverlordId, v => OverlordId = v);
        }

        public void SvOverlordChanged(int? id)
        {
            throw new NotImplementedException();
        }
    }
}