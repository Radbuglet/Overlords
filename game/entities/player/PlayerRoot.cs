using Godot;
using Overlords.game.entities.player.movement;
using Overlords.game.entities.shared;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player
{
	public class PlayerRoot : KinematicBody
	{
		[LinkNodeStatic("Logic/State")] public PlayerState State;
		[LinkNodeStatic("Logic/Shared")] public PlayerShared SharedLogic;
		[LinkNodeStatic("Head/Camera")] public Camera FpsCamera;
		[LinkNodeStatic("Logic/Movement/Net")] public PlayerMovementNet MovementNet;
		[LinkNodeStatic("Logic/Movement/Mover")] public HumanoidMover Mover;
		[LinkNodeStatic("Logic/Movement/Local")] public PlayerMovementLocal MovementLocal;

		public override void _EnterTree()
		{
			this.Initialize();
		}
	}
}
