using Godot;
using Overlords.game.entities.player.utils;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldClient : Node
    {
        [RequireBehavior] public WorldShared LogicShared;

        public override void _Ready()
        {
            this.InitializeBehavior();
            var entityReplicator = LogicShared.EntityReplicator;
            entityReplicator.AcceptingDynamicInstances = true;
            entityReplicator.ClRegisterBuilder((int) WorldProtocol.EntityType.Player, PlayerProtocol.NetworkConstructor.Serializer,
                constructor =>
            {
                // TODO
                GD.Print("Created puppet player!");
            });
        }
    }
}