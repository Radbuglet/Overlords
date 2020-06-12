using System.Collections.Generic;
using Godot;
using Overlords.game.entities.common;
using Overlords.game.entities.itemStack;
using Overlords.game.world;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.local
{
    public class PlayerGuiController: Node
    {
        [Export] private NodePath _pathToLeaderBoard;
        [Export] private NodePath _pathToInventoryGrid;
        
        [LinkNodeEditor(nameof(_pathToLeaderBoard))]
        public Control LeaderBoardRoot;
        
        [LinkNodeEditor(nameof(_pathToInventoryGrid))]
        public Node InventoryGrid;
        
        public PlayerLocal PlayerLocal => GetParent<PlayerLocal>();
        public Inventory Inventory => PlayerLocal.LogicShared.GetInventory();
        private readonly Dictionary<int, Control> _scoreboardEntries = new Dictionary<int, Control>();
        
        public override void _Ready()
        {
            this.InitializeNode();
            var sharedLogic = PlayerLocal.LogicShared;
            
            // Setup leader board
            var worldClient = sharedLogic.GetWorldClient();
            worldClient.Connect(nameof(WorldClient.PuppetPlayerAdded), this, nameof(_OnPlayerAdded));
            worldClient.Connect(nameof(WorldClient.PuppetPlayerRemoved), this, nameof(_OnPlayerRemoved));
            _OnPlayerAdded(PlayerLocal.GetGameObject<Node>());
            
            // Setup inventory
            sharedLogic.GetInventory().Connect(nameof(Inventory.SlotStackUpdated), this, nameof(_InventorySlotUpdated));
        }

        public override void _Process(float delta)
        {
            Inventory.InsertStack(new ItemStack
            {
                Material = (ItemMaterial) (GD.Randi() % 10),
                Amount = 43
            });
        }

        private void _OnPlayerAdded(Node playerRoot)
        {
            var entry = new Label{ Text = playerRoot.Name };
            LeaderBoardRoot.AddChild(entry);
            _scoreboardEntries.Add(playerRoot.GetBehavior<PlayerShared>().OwnerPeerId, entry);
        }

        private void _OnPlayerRemoved(Node playerRoot)
        {
            var peerId = playerRoot.GetBehavior<PlayerShared>().OwnerPeerId;
            _scoreboardEntries[peerId].Purge();
            _scoreboardEntries.Remove(peerId);
        }

        private void _InventorySlotUpdated(int slot)
        {
            var dataStack = Inventory.GetStackInSlot(slot);
            var uiStack = (TextureRect) InventoryGrid.GetChild(slot);
            uiStack.Texture = PlayerLocal.LogicShared.GetWorldClient().GetStackTexture(dataStack.Material);
        }
    }
}