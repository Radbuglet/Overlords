using System.Collections.Generic;
using Overlords.game.entities.player.common;
using Overlords.helpers.network.serialization;

namespace Overlords.game.world
{
    public static class WorldProtocol
    {
        public enum EntityType
        {
            Player
        }
        
        public enum ServerBound
        { }
        
        public enum ClientBound
        {
            Login
        }
        
        public class LoginPacket
        {
            public static readonly StructSerializer<LoginPacket> Serializer = new StructSerializer<LoginPacket>(
                () => new LoginPacket(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(LocalPlayer)] = PlayerProtocol.NetworkConstructor.Serializer,
                    [nameof(OtherEntities)] = new PrimitiveSerializer<Godot.Collections.Array>()
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
            
            public PlayerProtocol.NetworkConstructor LocalPlayer;
            public Godot.Collections.Array OtherEntities;
        }
    }
}