using Godot;
using Overlords.game.entities.player.mechanics;
using Overlords.game.entities.shared;
using Overlords.game.world.logic;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player
{
	public class PlayerRoot : KinematicBody
	{
		public WorldRoot WorldRoot => GetNode<WorldRoot>("../../");
		[LinkNodeStatic("Head")] public Spatial Head;
		[LinkNodeStatic("Head/Camera")] public Camera FpsCamera;
		[LinkNodeStatic("Head/Camera/RayCast")] public RayCast LookRayCast;
		[LinkNodeStatic("Logic/State")] public PlayerState State;
		[LinkNodeStatic("Logic/Shared")] public PlayerShared SharedLogic;
		[LinkNodeStatic("Logic/Movement/Net")] public PlayerMovementNet MovementNet;
		[LinkNodeStatic("Logic/Movement/Mover")] public HumanoidMover Mover;
		[LinkNodeStatic("Logic/LocalControls")] public PlayerControlsLocal ControlsLocal;
		[LinkNodeStatic("Logic/Interaction")] public PlayerInteraction Interaction;

		public override void _EnterTree()
		{
			this.Initialize();
		}
	}
}
