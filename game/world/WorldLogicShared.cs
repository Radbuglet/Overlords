using Godot;
using Overlords.game.entities.player;
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
        public ListReplicator EntityContainer;
        
        public readonly NodeGroup<int, Node> GroupPlayers = new NodeGroup<int, Node>();

        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityContainer.RegisterEntityType(PlayerPrefab, PlayerProtocol.NetworkConstructor.Serializer,
                (instance, container, constructor) =>
                {
                    var sharedLogic = instance.GetBehavior<PlayerLogicShared>();
                    sharedLogic.SetupPreEntry(GetTree(), constructor.OwnerPeerId);
                    sharedLogic.StateReplicator.LoadValues(constructor.ReplicatedState);
                    container.AddChild(instance);
                },
                (target, instance) => instance.GetBehavior<PlayerLogicServer>().MakeConstructor(target));
        }
    }
}