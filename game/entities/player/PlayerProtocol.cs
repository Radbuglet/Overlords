using System.Collections.Generic;
using Overlords.game.entities.player.character;
using Overlords.helpers.network.serialization;

namespace Overlords.game.entities.player
{
    public static class PlayerProtocol
    {
        public class NetworkConstructor
        {
            public static readonly StructSerializer<NetworkConstructor> Serializer =
                new StructSerializer<NetworkConstructor>(
                    () => new NetworkConstructor(),
                    new Dictionary<string, ISerializerRaw>
                    {
                        [nameof(OwnerPeerId)] = new PrimitiveSerializer<int>(),
                        [nameof(State)] = InitialState.Serializer,
                        [nameof(ReplicatedValues)] = new PrimitiveSerializer<Godot.Collections.Array>()
                    });

            public int OwnerPeerId;
            public InitialState State;
            public Godot.Collections.Array ReplicatedValues;
        }

        public class InitialState
        {
            public static readonly StructSerializer<InitialState> Serializer = new StructSerializer<InitialState>(
                () => new InitialState(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(CharacterState)] =
                        new OptionalSerializer<CharacterProtocol.InitialState>(
                            CharacterProtocol.InitialState.Serializer)
                });
            
            public CharacterProtocol.InitialState CharacterState;
        }
    }
}