using System.Collections.Generic;
using Godot;
using Overlords.game.entities.player;
using Overlords.game.entities.player.client;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicShared: Node
    {
        [Export][FieldNotNull]
        public PackedScene PlayerPrefab;
        
        [LinkNodeStatic("../EntityContainer")]
        public ListReplicator Entities;
        
        public readonly NodeGroup<int, Node> Players = new NodeGroup<int, Node>();

        public IEnumerable<int> GetPlayingPeers()
        {
            return Players.IterateGroupKeys();
        }
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            Entities.RegisterEntityType(PlayerPrefab, PlayerProtocol.NetworkConstructor.Serializer,
                (instance, container, constructor) =>
                {
                    var sharedLogic = instance.GetBehavior<PlayerLogicShared>();
                    sharedLogic.SetupPreEntry(GetTree(), GetParent(), constructor.OwnerPeerId, constructor.State);
                    container.AddChild(instance);
                },
                (target, instance) => instance.GetBehavior<PlayerLogicServer>().MakeConstructor(target));
        }
    }
}