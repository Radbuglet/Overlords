using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.player;
using Overlords.helpers.tree.trackingGroup;

namespace Overlords.game.world.logic
{
    public class WorldShared : Node
    {
        private WorldRoot WorldRoot => GetNode<WorldRoot>("../../");
        
        public readonly NodeGroup<string, Node> InteractionTargets = new NodeGroup<string, Node>();
        public readonly NodeGroup<int, PlayerRoot> Players = new NodeGroup<int, PlayerRoot>();

        public override void _Ready()
        {
            foreach (var node in GetTree().GetNodesInGroup(EntityTypes.RegisterInteractionGroupName).Cast<Node>())
            {
                InteractionTargets.AddToGroup(node.Name, node);
            }
        }

        public IEnumerable<int> GetOnlinePeers()
        {
            return Players.IterateGroupMembers()
                .Select(player => player.State.OwnerPeerId.Value);
        }

        public void RegisterPlayer(PlayerRoot root)
        {
            Players.AddToGroup(root.State.OwnerPeerId.Value, root);
        }

        public PlayerRoot GetPlayer(int peerId)
        {
            return Players.GetMemberOfGroup<PlayerRoot>(peerId, null);
        }
    }
}