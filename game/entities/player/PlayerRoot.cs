using Godot;
using Overlords.game.entities.player.gui;
using Overlords.game.entities.player.inventory;
using Overlords.game.entities.player.mechanics;
using Overlords.game.entities.shared;
using Overlords.game.world.logic;
using Overlords.helpers.tree;

namespace Overlords.game.entities.player
{
	public class PlayerRoot : KinematicBody
	{
		// TODO: Use GetParent() instead of GetNode() to avoid the performance hit caused by parsing NodePaths
		public WorldRoot WorldRoot => GetNode<WorldRoot>("../../");
		
		// TODO: Not all of these have to be loaded by the root node.
		[LinkNodeStatic("Head")] public Spatial Head;
		[LinkNodeStatic("Head/Camera")] public Camera FpsCamera;
		[LinkNodeStatic("Head/Camera/RayCast")] public RayCast LookRayCast;
		[LinkNodeStatic("Inventory")] public PlayerInventory Inventory;
		[LinkNodeStatic("Gui")] public GuiController GuiController;
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
