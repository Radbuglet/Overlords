using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.constants;
using Overlords.game.entities.player;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldLogicShared : Node
    {
        [LinkNodeStatic("../EntityContainer")] public ListReplicator Entities;
        [Export] [FieldNotNull] public PackedScene PlayerPrefab;
        public readonly NodeGroup<int, Node> Players = new NodeGroup<int, Node>();
        public readonly NodeGroup<string, Node> Targets = new NodeGroup<string, Node>();

        public IEnumerable<int> GetPlayingPeers()
        {
            return Players.IterateGroupKeys();
        }

        public IEnumerable<int> GetPlayingPeers(int ignorePeerId)
        {
            return GetPlayingPeers().Where(peerId => ignorePeerId != peerId);
        }

        public override void _Ready()
        {
            this.InitializeBehavior();
            Entities.RegisterEntityType(PlayerPrefab, PlayerProtocol.NetworkConstructor.Serializer,
                (instance, container, constructor) =>
                {
                    var sharedLogic = instance.GetBehavior<PlayerLogicShared>();
                    sharedLogic.Initialize(GetTree(), GetParent(), constructor.OwnerPeerId, constructor.State);
                    sharedLogic.CatchupState(constructor.ReplicatedValues);
                    container.AddChild(instance);
                },
                (target, instance) => instance.GetBehavior<PlayerLogicServer>().MakeConstructor(target));

            // TODO: Auto register static interaction targets
        }
    }
}