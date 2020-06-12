using System;
using System.Collections.Generic;
using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.common;
using Overlords.game.world;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.local
{
    public class PlayerGuiController: Node
    {
        public enum Screen
        {
            None,
            Inventory,
            Pause
        }
        
        [Export] private NodePath _pathToLeaderBoard;
        [Export] private NodePath _pathToInventoryRoot;
        [Export] private NodePath _pathToInventoryGrid;
        
        [LinkNodeEditor(nameof(_pathToLeaderBoard))]
        public Control LeaderBoardRoot;
        
        [LinkNodeEditor(nameof(_pathToInventoryRoot))]
        public Control InventoryGuiRoot;
        
        [LinkNodeEditor(nameof(_pathToInventoryGrid))]
        public Node InventoryGuiGrid;
        
        public PlayerLocal PlayerLocal => GetParent<PlayerLocal>();
        public Inventory Inventory => PlayerLocal.LogicShared.GetInventory();
        private readonly Dictionary<int, Control> _scoreboardEntries = new Dictionary<int, Control>();
        private Screen _screen = Screen.None;
        
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
            ResetMenuState();
        }
        
        // ReSharper disable InvertIf
        public override void _Process(float delta)
        {
            switch (_screen)
            {
                case Screen.None:
                {
                    if (GameInputs.FpsInventory.WasJustPressed())
                    {
                        InventoryGuiRoot.Visible = true;
                        _screen = Screen.Inventory;
                        Input.SetMouseMode(Input.MouseMode.Visible);
                    }
                
                
                    if (GameInputs.FpsClose.WasJustPressed())
                    {
                        _screen = Screen.Pause;
                        Input.SetMouseMode(Input.MouseMode.Visible);
                    }

                    break;
                }
                case Screen.Inventory:
                    if (GameInputs.FpsInventory.WasJustPressed() || GameInputs.FpsClose.WasJustPressed())
                    {
                        InventoryGuiRoot.Visible = false;
                        ResetMenuState();
                    }
                    break;
                case Screen.Pause:
                    if (GameInputs.FpsClose.WasJustPressed())
                    {
                        ResetMenuState();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        // ReSharper restore InvertIf

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
            var uiStack = (TextureRect) InventoryGuiGrid.GetChild(slot);
            uiStack.Texture = PlayerLocal.LogicShared.GetWorldClient().GetStackTexture(dataStack.Material);
        }
        
        private void ResetMenuState()
        {
            _screen = Screen.None;
            Input.SetMouseMode(Input.MouseMode.Captured);
        }
        
        public bool HasControl()
        {
            return _screen == Screen.None;
        }
    }
}