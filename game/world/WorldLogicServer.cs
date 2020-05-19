using Godot;
using Overlords.game.entities.player;
using Overlords.helpers;
using Overlords.helpers.csharp;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.interfaceBehaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public interface IEntityCatchupEmitter
    {
        object SerializeConstructor(int target);
    }
    
    public class WorldLogicServer : Node
    {
        [Export]
        private PackedScene _playerPrefab;
        
        [RequireBehavior]
        public WorldLogicShared SharedLogic;
        
        [LinkNodeStatic("../EntityContainer")]
        public ListReplicator EntityContainer;
        
        public readonly NodeGroup<string, Node> GroupAutoCatchup = new NodeGroup<string, Node>();
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityContainer.SerializeInstance = (target, entity) => new Protocol.ReplicatedEntity
            {
                TypeIndex = SharedLogic.TypeRegistrar.GetTypeFromNode(entity).Index,
                Constructor = entity.GetImplementation<IEntityCatchupEmitter>().SerializeConstructor(target)
            };
            
            var tree = GetTree();
            tree.Connect(SceneTreeSignals.NetworkPeerConnected, this, nameof(_PeerJoined));
            tree.Connect(SceneTreeSignals.NetworkPeerDisconnected, this, nameof(_PeerLeft));
        }

        private void RegisterAutoCatchup(Node node)
        {
            GroupAutoCatchup.AddToGroup(node.Name, node);
        }

        private void _PeerJoined(int peerId)
        {
            GD.Print($"{peerId} joined!");
            
            // Create and setup player
            var newPlayer = _playerPrefab.Instance();
            newPlayer.GetBehavior<PlayerLogicShared>()
                .InitializeShared(peerId);
            EntityContainer.AddChild(newPlayer);
            RegisterAutoCatchup(newPlayer);
            
            // Replicate player
            EntityContainer.SvReplicateInstance(SharedLogic.GroupPlayers.IterateGroupKeys(), newPlayer);
            SharedLogic.GroupPlayers.AddToGroup(peerId, newPlayer);
            EntityContainer.SvReplicateInstances(peerId, GroupAutoCatchup.IterateGroupMembers());
        }
        
        private void _PeerLeft(int peerId)
        {
            GD.Print($"{peerId} left!");

            var playerNodeGroup = SharedLogic.GroupPlayers;
            var player = playerNodeGroup.GetMemberOfGroup<Node>(peerId, this);
            if (player == null) return;
            
            playerNodeGroup.RemoveFromGroup(player);
            EntityContainer.SvDeReplicateInstances(playerNodeGroup.IterateGroupKeys(), player.AsEnumerable());
            player.Purge();
        }
    }
}