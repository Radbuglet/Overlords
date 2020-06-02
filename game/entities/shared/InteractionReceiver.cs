using Godot;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.entities.shared
{
	public class InteractionReceiver : Node
	{
		public override void _Ready()
		{
			this.InitializeBehavior();
		}

		public void OnTrigger()
		{
			GD.Print("We got interacted with!");
		}
	}
}
