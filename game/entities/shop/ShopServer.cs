using Godot;
using Overlords.game.constants;
using Overlords.game.entities.player;
using Overlords.game.entities.player.character;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.shop
{
	public class ShopServer: Node
	{
		public override void _Ready()
		{
			this.InitializeBehavior();
		}

		[BindEntitySignal(nameof(GameSignals.OnEntityInteracted))]
		private void _OnInteracted(Node characterRoot)
		{
			var characterShared = characterRoot.GetBehavior<CharacterLogicShared>();
			var playerShared = characterShared.PlayerRoot.GetBehavior<PlayerLogicShared>();
			var playerServer = playerShared.GetGameObject<Node>().GetBehavior<PlayerLogicServer>();
			GD.Print("Shop interacted with.");
			playerServer.SetBalanceReplicated(playerShared.Balance.Value - 20);
		}
	}
}
