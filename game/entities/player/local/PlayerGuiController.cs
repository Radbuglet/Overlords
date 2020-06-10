using System.Collections.Generic;
using Godot;
using Overlords.game.entities.common;
using Overlords.game.world;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.local
{
    public class PlayerGuiController: Node
    {
        [Export] private NodePath _pathToLeaderBoard;
        [LinkNodeEditor(nameof(_pathToLeaderBoard))]
        public Control LeaderBoardRoot;
        
        public PlayerLocal PlayerLocal => GetParent<PlayerLocal>();
        private readonly Dictionary<int, Control> _scoreboardEntries = new Dictionary<int, Control>();
        
        public override void _Ready()
        {
            this.InitializeNode();
            var worldClient = PlayerLocal.LogicShared.GetWorldClient();
            worldClient.Connect(nameof(WorldClient.PuppetPlayerAdded), this, nameof(OnPlayerAdded));
            worldClient.Connect(nameof(WorldClient.PuppetPlayerRemoved), this, nameof(OnPlayerRemoved));
            OnPlayerAdded(PlayerLocal.GetGameObject<Node>());
        }

        public void OnPlayerAdded(Node playerRoot)
        {
            var entry = new Label{ Text = playerRoot.Name };
            LeaderBoardRoot.AddChild(entry);
            _scoreboardEntries.Add(playerRoot.GetBehavior<PlayerShared>().OwnerPeerId, entry);
        }

        public void OnPlayerRemoved(Node playerRoot)
        {
            var peerId = playerRoot.GetBehavior<PlayerShared>().OwnerPeerId;
            _scoreboardEntries[peerId].Purge();
            _scoreboardEntries.Remove(peerId);
        }
    }
}