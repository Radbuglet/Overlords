using Godot;
using Overlords.game.constants;
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
		private void _OnInteracted()
		{
			GD.Print("Shop interacted with!");
		}
	}
}
