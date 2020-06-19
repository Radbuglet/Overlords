﻿using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.entities.player;

namespace Overlords.game.definitions
{
    public static class EntityTypes
    {
        public delegate void SignalOnInteracted(PlayerRoot player);
        public const string PlayersGroupName = "players";
        public const string InteractableGroupName = "interactable";

        public static IEnumerable<int> GetPlayingPeers(this SceneTree tree)
        {
            return tree.GetNodesInGroup(PlayersGroupName).Cast<PlayerRoot>()
                .Select(player => player.State.OwnerPeerId.Value);
        }

        public static string GetPlayerName(int peerId)
        {
            return $"player_{peerId}";
        }
    }
}