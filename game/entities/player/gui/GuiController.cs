using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player.gui
{
    public enum GuiState
    {
        Playing,
        InGameMenu,
        Paused
    }
    
    public class GuiController : Node, IValidationAwaiter
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
            this.FlagAwaiter();
            foreach (var child in GetChildren().Cast<Node>())
            {
                if (child is Control control)
                    control.Visible = false;
            }
        }
        
        public void _StateValidated()
        {
            SwapStatePlaying();
        }

        public override void _Input(InputEvent @event)
        {
            if (GameInputs.FpsPause.WasJustPressed())
            {
                if (State == GuiState.Paused || State == GuiState.InGameMenu)
                {
                    SwapStatePlaying();
                }
                else
                {
                    SwapStatePaused();
                }   
            } else if (GameInputs.FpsInventory.WasJustPressed())
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