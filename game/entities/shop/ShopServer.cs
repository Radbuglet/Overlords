using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.player;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.shop
{
	public class ShopServer: Node
	{
		[RequireBehavior] public ShopShared ShopShared;
		
		public override void _Ready()
		{
			this.InitializeBehavior();
		}

		[BindEntitySignal(nameof(GameSignals.OnEntityInteracted))]
		private void _OnInteracted(string myTargetId, Node playerRoot)
		{
			GD.Print($"Shop interacted with by {playerRoot.Name}.");
			ShopShared.PerformTransaction(playerRoot.GetBehavior<PlayerShared>());
			playerRoot.GetBehavior<PlayerServer>().SendTransactionComplete(myTargetId);
		}
	}
}
