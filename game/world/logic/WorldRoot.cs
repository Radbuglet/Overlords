using Godot;
using Overlords.game.world.entityCore;
using Overlords.helpers.tree;

namespace Overlords.game.world.logic
{
	public class WorldRoot : Node
	{
		[LinkNodeStatic("Entities")] public ListReplicator Entities;
		[LinkNodeStatic("Logic/Shared")] public WorldShared Shared;
		[LinkNodeStatic("Logic/LoginHandler")] public LoginHandler LoginHandler;
		
		public override void _EnterTree()
		{
			this.Initialize();
		}
	}
}
