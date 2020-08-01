using Godot;
using Overlords.game.player.gui;
using Overlords.game.player.inventory;
using Overlords.game.player.mechanics;
using Overlords.game.shared;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;

namespace Overlords.game.player
{
	public class PlayerRoot : KinematicBody
	{
		public GameRoot Game => this.GetScene<GameRoot>();
		
		[LinkNodeStatic("Head")] public Spatial Head;
		[LinkNodeStatic("Head/Camera")] public Camera FpsCamera;
		[LinkNodeStatic("Head/Camera/RayCast")] public RayCast LookRayCast;
		[LinkNodeStatic("Inventory")] public PlayerInventory Inventory;
		[LinkNodeStatic("Gui")] public GuiController GuiController;
		[LinkNodeStatic("Logic/State")] public PlayerState State;
		[LinkNodeStatic("Logic/Shared")] public PlayerShared Shared;
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
