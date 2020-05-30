using System.Collections.Generic;
using Godot;
using Overlords.game.entities.player.character;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.player
{
    public static class PlayerProtocol
    {
        public class NetworkConstructor
        {
            public static readonly StructSerializer<NetworkConstructor> Serializer = new StructSerializer<NetworkConstructor>(
                () => new NetworkConstructor(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>(),
                    [nameof(State)] = InitialState.Serializer
                });

            public int OwnerPeerId;
            public InitialState State;
        }
        
        public class InitialState
        {
            public static readonly StructSerializer<InitialState> Serializer = new StructSerializer<InitialState>(
                () => new InitialState(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(Balance)] = new PrimitiveSerializer<int>(),
                    [nameof(CharacterState)] = new OptionalSerializer<CharacterProtocol.InitialState>(CharacterProtocol.InitialState.Serializer)
                });

            public int Balance;
            public CharacterProtocol.InitialState CharacterState;
        }
    }
}