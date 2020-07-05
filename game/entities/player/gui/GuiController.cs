using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player.gui
{
    public enum GudState
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

        public GudState State { get; private set; } = GudState.Playing;
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
            if (!GameInputs.FpsPause.WasJustPressed()) return;
            
            if (State == GudState.Paused)
            {
                SwapStatePlaying();
            }
            else
            {
                SwapStatePaused();
            }
        }

        private void SwapState(GudState state, Control newUiRoot)
        {
            // Swap UI roots
            if (CurrentUiRoot != null)
                CurrentUiRoot.Visible = false;

            newUiRoot.Visible = true;
            CurrentUiRoot = newUiRoot;
            
            // Set variables
            State = state;
            Input.SetMouseMode(state == GudState.Playing ? Input.MouseMode.Captured : Input.MouseMode.Visible);
        }

        public void SwapStatePlaying()
        {
            SwapState(GudState.Playing, HudRoot);
        }

        public void SwapStatePaused()
        {
            SwapState(GudState.Paused, PauseRoot);
        }

        public void SwapStateMenu(Control menuRoot)
        {
            SwapState(GudState.InGameMenu, menuRoot);
        }
    }
}