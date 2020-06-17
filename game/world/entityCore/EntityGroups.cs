using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Overlords.game.world.entityCore
{
    public static class EntityTypes
    {
        public const string PlayersGroupName = "players";
        
        public static IEnumerable<int> GetPlayingPeers(this SceneTree tree)
        {
            return tree.GetNodesInGroup(PlayersGroupName).Cast<Node>()
                .Select(player => 2);  // TODO: Implement
        }
    }
}