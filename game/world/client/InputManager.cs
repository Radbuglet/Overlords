using Godot;

namespace Overlords.game.world.client
{
	public class InputManager : Node
	{
		[Export] private GameStates _gameState = GameStates.Paused;
		public enum GameStates
		{
			Playing,
			Paused
		}

		public GameStates GameState
		{
			get => _gameState;
			set
			{
				Input.SetMouseMode(_gameState == GameStates.Playing ? Input.MouseMode.Captured : Input.MouseMode.Visible);
				_gameState = value;
			}
		}

		public override void _Ready()
		{
			GameState = _gameState;
		}

		public bool CharacterHasControl()
		{
			return _gameState == GameStates.Playing;
		}
	}
}
