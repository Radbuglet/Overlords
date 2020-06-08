using System.Collections.Generic;
using System.Linq;
using Godot;
using Overlords.game.constants;
using Overlords.game.entities.player;
using Overlords.game.entities.player.utils;
using Overlords.helpers.network.replication;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.behaviors;
using Overlords.helpers.tree.trackingGroups;

namespace Overlords.game.world
{
    public class WorldShared : Node
    {
        [LinkNodeStatic("../EntityContainer")] public ListReplicator EntityReplicator;
        [Export] [FieldNotNull] public PackedScene PlayerPrefab;
        public readonly NodeGroup<int, Node> Players = new NodeGroup<int, Node>();
        public readonly NodeGroup<string, Spatial> InteractionTargets = new NodeGroup<string, Spatial>();

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
            EntityReplicator.RegisterEntityType(PlayerPrefab, PlayerProtocol.NetworkConstructor.Serializer,
                (instance, container, constructor) =>
                {
                    try
                    {
                        var sharedLogic = instance.GetBehavior<PlayerShared>();
                        sharedLogic.InitializeRemote(GetTree(), this.GetGameObject<Node>(), constructor);
                        container.AddChild(instance);
                    }
                    catch (DeserializationException e)
                    {
                        GD.PushWarning($"Failed to spawn player: {e.Message}");
                    }
                },
                (target, instance) => instance.GetBehavior<PlayerServer>().MakeConstructor(target));

            // Auto register static interaction targets (happens on both)
            foreach (var node in GetTree().GetNodesInGroup(GameGdGroups.StaticInteractionTarget).Cast<Spatial>())
            {
                InteractionTargets.AddToGroup($"static_{node.Name}", node);
            }
        }
    }
}