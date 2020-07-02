using Godot;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player.inventory
{
	public class ItemStackRoot : Node
	{
		[LinkNodeStatic("State")] public ItemStackState State;

		public override void _Ready()
		{
			this.Initialize();
		}
	}
}
