using Godot;
using Overlords.helpers.network;

namespace Overlords.helpers.tree.conditionals
{
    public class NetworkConditionalNode : ConditionalNode
    {
        [Export] private readonly NetworkTypeUtils.NetworkMode _desiredNetworkMode;

        protected override bool ShouldExist()
        {
            return this.GetNetworkMode() == _desiredNetworkMode;
        }
    }
}