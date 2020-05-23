using System.Collections.Generic;
using Godot;
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
                    [nameof(State)] = PlayerInitialState.Serializer
                });

            public int OwnerPeerId;
            public PlayerInitialState State;
        }
        
        public class PlayerInitialState
        {
            public static readonly StructSerializer<PlayerInitialState> Serializer = new StructSerializer<PlayerInitialState>(
                () => new PlayerInitialState(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(Balance)] = new PrimitiveSerializer<int>(),
                    [nameof(HasCharacter)] = new PrimitiveSerializer<bool>(),
                    [nameof(CharacterState)] = CharacterInitialState.Serializer
                }, data =>
                {
                    if (data.HasCharacter != (data.CharacterState != null))
                        throw new DeserializationException("HasCharacter doesn't agree with CharacterState presence!");
                });

            public int Balance;
            public bool HasCharacter;
            public CharacterInitialState CharacterState;
        }
        
        public class CharacterInitialState
        {
            public static readonly StructSerializer<CharacterInitialState> Serializer = new StructSerializer<CharacterInitialState>(
                () => new CharacterInitialState(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(Position)] = new PrimitiveSerializer<Vector3>()
                });

            public Vector3 Position;
        }
    }
}