using Godot;
using Overlords.helpers.network;

namespace Overlords.helpers.conditionals
{
    public class NetworkConditionalNode: ConditionalNode
    {
        [Export] private readonly NetworkUtils.NetworkMode _desiredNetworkMode;
        
        protected override bool ShouldExist()
        {
            return this.GetNetworkMode() == _desiredNetworkMode;
        }
    }
}