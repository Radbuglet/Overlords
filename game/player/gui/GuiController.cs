using System.Linq;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.player.gui
{
    public enum GuiState
    {
        Playing,
        InGameMenu,
        Paused
    }
    
    public class GuiController : Node, ICatchupAwaiter
    {
        [LinkNodeStatic("Hud")]
        public Control HudRoot;

        [LinkNodeStatic("Pause")]
        public Control PauseRoot;
        
        [LinkNodeStatic("Inventory")]
        public Control InventoryRoot;

        public GuiState State { get; private set; } = GuiState.Playing;
        public Control CurrentUiRoot;

        public override void _Ready()
        {
            this.Initialize();
            this.FlagCatchupAwaiter();
            foreach (var child in GetChildren().Cast<Node>())
            {
                if (child is Control control)
                    control.Visible = false;
            }
        }

        public void _CaughtUp()
        {
            SwapStatePlaying();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("fps_pause"))
            {
                if (State == GuiState.Paused || State == GuiState.InGameMenu)
                {
                    SwapStatePlaying();
                }
                else
                {
                    SwapStatePaused();
                }   
            } else if (Input.IsActionJustPressed("fps_inventory"))
            {
                if (State == GuiState.Playing)
                {
                    SwapStateMenu(InventoryRoot);
                }
                else if (State == GuiState.InGameMenu)
                {
                    SwapStatePlaying();
                }
            }
        }

        private void SwapState(GuiState state, Control newUiRoot)
        {
            // Swap UI roots
            if (CurrentUiRoot != null)
                CurrentUiRoot.Visible = false;

            newUiRoot.Visible = true;
            CurrentUiRoot = newUiRoot;
            
            // Set variables
            State = state;
            Input.SetMouseMode(state == GuiState.Playing ? Input.MouseMode.Captured : Input.MouseMode.Visible);
        }

        public void SwapStatePlaying()
        {
            SwapState(GuiState.Playing, HudRoot);
        }

        public void SwapStatePaused()
        {
            SwapState(GuiState.Paused, PauseRoot);
        }

        public void SwapStateMenu(Control menuRoot)
        {
            SwapState(GuiState.InGameMenu, menuRoot);
        }
    }
}