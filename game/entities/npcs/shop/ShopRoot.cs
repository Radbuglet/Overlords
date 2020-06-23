using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.player;

namespace Overlords.game.entities.npcs.shop
{
	public class ShopRoot : StaticBody
	{
		public override void _Ready()
		{
			AddUserSignal(nameof(EntityTypes.SignalOnInteracted));
			Connect(nameof(EntityTypes.SignalOnInteracted), this, nameof(_InteractedWith));
		}

		// TODO: Should really be server exclusive
		private void _InteractedWith(PlayerRoot playerRoot)
		{
			GD.Print("Interacted with!");
		}
	}
}
