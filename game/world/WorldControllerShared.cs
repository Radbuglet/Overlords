using Godot;
using Overlords.game.entity.player;
using Overlords.helpers.network.replication;
using Overlords.helpers.tree.behaviors;

namespace Overlords.game.world
{
    public class WorldControllerShared: Node
    {
        [FieldNotNull] [Export] private PackedScene _playerPrefab;
        
        [LinkNodeStatic("../../EntityContainer")]
        public EntityContainer EntityContainer;
        
        public EntityContainer.RegisteredEntityType EntityTypePlayer;
        
        public override void _Ready()
        {
            this.InitializeBehavior();
            EntityTypePlayer = EntityContainer.RegisterEntityType(Protocol.PlayerConstructor.Serializer,
                constructor =>
                {
                    var player = _playerPrefab.Instance();
                    player.Name = Protocol.GetPlayerName(constructor.OwnerPeerId);
                    player.GetBehavior<PlayerInitializer>().Initialize(constructor);
                    EntityContainer.AddChild(player);
                }, (target, player) => new Protocol.PlayerConstructor());
        }
    }
}