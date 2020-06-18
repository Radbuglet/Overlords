using Godot;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player
{
	public class PlayerRoot : KinematicBody
	{
		[LinkNodeStatic("Logic/State")] public PlayerState State;
		[LinkNodeStatic("Logic/Shared")] public PlayerShared SharedLogic;

		public override void _EnterTree()
		{
			this.Initialize();
		}
	}
}
