using Godot;
using Overlords.game.world.shared;
using Overlords.helpers.behaviors;
// ReSharper disable UnassignedField.Global

namespace Overlords.game.entity.player
{
    public class PublicPlayerState: Node
    {
        [RequireParent] public Spatial PlayerSpatialRoot;
        
        [Export] public string PlayerName;
        [Export] public Vector2 Orientation;
        [Export] public int Balance;

        public override void _Ready()
        {
            this.InitializeBehavior();
        }

        public void ApplyInfo(Protocol.PlayerInfoPublic infoPacket)
        {
            PlayerName = infoPacket.Name;
            PlayerSpatialRoot.Translation = infoPacket.Position;
            Orientation = infoPacket.Orientation;
            Balance = infoPacket.Balance;
        }

        public Protocol.PlayerInfoPublic SerializeInfo(int peerId)
        {
            return new Protocol.PlayerInfoPublic
            {
                PeerId = peerId,
                Name = Name,
                Position = PlayerSpatialRoot.Translation,
                Orientation = Orientation,
                Balance = Balance
            };
        }
    }
}