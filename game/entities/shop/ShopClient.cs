using Godot;
using Overlords.game.constants;
using Overlords.game.entities.player.character;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.shop
{
	public class ShopClient: Node
	{
		public override void _Ready()
		{
			this.InitializeBehavior();
		}

		[BindEntitySignal(nameof(GameSignals.OnEntityInteracted))]
		private void _OnInteracted(Node characterRoot)
		{
			var characterShared = characterRoot.GetBehavior<CharacterLogicShared>();
			var playerShared = characterShared.PlayerShared;
			GD.Print($"Shop interacted with. Their balance: {playerShared.Balance.Value}");
		}
	}
}
