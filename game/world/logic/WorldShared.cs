using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.player;
using Overlords.helpers.tree;

namespace Overlords.game.world.logic
{
    public class WorldShared : Node
    {
        private WorldRoot WorldRoot => GetNode<WorldRoot>("../../");
        
        public readonly NodeDictionary<string, Node> InteractionTargets = new NodeDictionary<string, Node>();
        public readonly NodeDictionary<int, PlayerRoot> Players = new NodeDictionary<int, PlayerRoot>();

        public override void _Ready()
        {
            foreach (var node in GetTree().GetNodesInGroup(EntityTypes.RegisterInteractionGroupName).Cast<Node>())
            {
                InteractionTargets.Add(node.Name, node);
            }
        }

        public IEnumerable<int> GetOnlinePeers()
        {
            return Players.GetValues()
                .Select(player => player.State.OwnerPeerId.Value);
        }

        public void RegisterPlayer(PlayerRoot root)
        {
            Players.Add(root.State.OwnerPeerId.Value, root);
        }

        public PlayerRoot GetPlayer(int peerId)
        {
            return Players.TryGetValue<PlayerRoot>(peerId, out var player) ? player : null;
        }
    }
}