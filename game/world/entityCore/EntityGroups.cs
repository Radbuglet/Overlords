﻿using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.entities.player;

namespace Overlords.game.world.entityCore
{
    public static class EntityTypes
    {
        public const string PlayersGroupName = "players";
        
        public static IEnumerable<int> GetPlayingPeers(this SceneTree tree)
        {
            return tree.GetNodesInGroup(PlayersGroupName).Cast<PlayerRoot>()
                .Select(player => player.State.OwnerPeerId.Value);
        }
    }
}