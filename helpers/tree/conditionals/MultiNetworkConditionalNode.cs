using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.services;

namespace Overlords.helpers.tree.conditionals
{
	public class MultiNetworkConditionalNode : Node, IParentEnterTrigger
	{
		[Export] private Array<NodePath> _clientNodes;
		[Export] private Array<NodePath> _serverNodes;

		public void _EarlyEditorTrigger(SceneTree tree)
		{
			NodePurging.PurgeParallel(
				(tree.GetNetworkMode() == NetworkTypeUtils.NetworkMode.Server ? _clientNodes : _serverNodes)
				.ConvertToNodeIterator(this));
			this.Purge(); // We're not locked thanks to _EarlyEditorTrigger()
		}
	}
}