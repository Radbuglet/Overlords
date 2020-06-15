using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.definitions;
using Overlords.game.entities.itemStack;
using Overlords.helpers.network;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldShared : Node
    {
        [Export] [FieldNotNull] public PackedScene PlayerPrefab;
        [Export] [FieldNotNull] public PackedScene ItemStackPrefab;
        [LinkNodeStatic("../RemoteEvent")] public RemoteEvent RemoteEvent;
        [LinkNodeStatic("../EntityContainer")] public ListReplicator EntityReplicator;
        
        public readonly NodeGroup<int, Node> Players = new NodeGroup<int, Node>();
        public readonly NodeGroup<string, Spatial> InteractionTargets = new NodeGroup<string, Spatial>();
        
        public int? ActiveOverlordPeer;

        public override void _Ready()
        {
            this.InitializeBehavior();
            foreach (var node in GetTree().GetNodesInGroup(GameGdGroups.StaticInteractionTarget).Cast<Spatial>())
            {
                InteractionTargets.AddToGroup($"static_{node.Name}", node);
            }
        }

        public ItemStack MakeItemStack(ItemMaterial material, int amount)
        {
            var stack = (ItemStack) ItemStackPrefab.Instance();
            stack.Material = material;
            stack.Amount = amount;
            return stack;
        }
        
        public IEnumerable<int> GetPlayingPeers()
        {
            return Players.IterateGroupKeys();
        }
        
        public IEnumerable<int> GetPlayingPeers(int ignorePeerId)
        {
            return GetPlayingPeers().Where(peerId => ignorePeerId != peerId);
        }
    }
}