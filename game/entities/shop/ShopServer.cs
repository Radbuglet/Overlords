using Godot;
using Overlords.game.constants;
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
		private void _OnInteracted(Node playerRoot)
		{
			GD.Print($"Shop interacted with by {playerRoot.Name}.");
		}
	}
}
