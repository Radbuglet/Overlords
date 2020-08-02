using System.Diagnostics;
using Godot;
using Overlords.game.player;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.replication;

namespace Overlords.game
{
    public class GameState : StateConstructor
    {
        private GameRoot Game => this.GetScene<GameRoot>();
        
        public int? OverlordId;
        
        public GameState()
        {
            AddField(() => OverlordId, v => OverlordId = v, true);
        }

        public PlayerRoot GetOverlord()
        {
            return OverlordId != null && Game.OnlinePlayers.TryGetValue(OverlordId.Value, out var overlord) ?
                overlord : null;
        }

        public void SvOverlordChanged(int? id)
        {
            Debug.Assert(this.GetNetworkMode() == NetworkMode.Server);
            OverlordId = id;
            GetOverlord().MovementNet.Teleport(Game.OverlordSpawnPoint.GetGlobalPosition());
            this.RpcVis(nameof(_NewOverlord), id != null, id ?? 0);
        }

        [Remote]
        private void _NewOverlord(bool hasOverlord, int id)
        {
            OverlordId = hasOverlord ? id : (int?) null;
            GD.Print("New Overlord!");
        }
    }
}