using System;
using Godot;
using Overlords.game.definitions;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.player.local
{
    public class PlayerGuiScreens: Node
    {
        public enum Screen
        {
            None,
            Inventory,
            Pause
        }
        
        [Export] private NodePath _pathToHudRoot;
        [Export] private NodePath _pathToInventoryRoot;
        [Export] private NodePath _pathToPauseRoot;
        
        [LinkNodeEditor(nameof(_pathToHudRoot))]
        public Control HudRoot;
        
        [LinkNodeEditor(nameof(_pathToInventoryRoot))]
        public Control InventoryRoot;
        
        [LinkNodeEditor(nameof(_pathToPauseRoot))]
        public Control PauseRoot;
        
        public Screen CurrentScreen { get; private set; } = Screen.None;

        public override void _Ready()
        {
            this.InitializeBehavior();
            UpdateView();
        }

        public override void _Process(float delta)
        {
            switch (CurrentScreen)
            {
                case Screen.None:
                    if (GameInputs.FpsInventory.WasJustPressed())
                    {
                        CurrentScreen = Screen.Inventory;
                        UpdateView();
                    } else if (GameInputs.FpsPause.WasJustPressed())
                    {
                        CurrentScreen = Screen.Pause;
                        UpdateView();
                    }
                    break;
                case Screen.Inventory:
                case Screen.Pause:
                    if (GameInputs.FpsPause.WasJustPressed() || CurrentScreen == Screen.Inventory && GameInputs.FpsInventory.WasJustPressed())
                    {
                        CurrentScreen = Screen.None;
                        UpdateView();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateView()
        {
            // TODO: Animate screens
            HudRoot.Visible = CurrentScreen == Screen.None;
            InventoryRoot.Visible = CurrentScreen == Screen.Inventory;
            PauseRoot.Visible = CurrentScreen == Screen.Pause;
            Input.SetMouseMode(CurrentScreen == Screen.None ? Input.MouseMode.Captured : Input.MouseMode.Visible);
        }
    }
}