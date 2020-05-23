using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Overlords.helpers.tree.behaviors;

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
                    [nameof(CharacterState)] = new OptionalSerializer<CharacterInitialState>(CharacterInitialState.Serializer)
                });

            public int Balance;
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
    
    public static class PlayerUtils
    {
        public static void InitializeControlledVariant(SceneTree tree, Node instanceRoot, int ownerPeerId,
            Type behaviorServer, Type behaviorClientShared, Type behaviorClientLocal, Type behaviorClientPuppet)
        {
            if (tree.GetNetworkMode() == NetworkUtils.NetworkMode.Server)
            {
                instanceRoot.GetBehaviorDynamic(behaviorClientShared).Purge();
                instanceRoot.GetBehaviorDynamic(behaviorClientLocal).Purge();
                instanceRoot.GetBehaviorDynamic(behaviorClientPuppet).Purge();
            }
            else
            {
                instanceRoot.GetBehaviorDynamic(behaviorServer).Purge();
                if (ownerPeerId == tree.GetNetworkUniqueId())
                {
                    instanceRoot.GetBehaviorDynamic(behaviorClientPuppet).Purge();
                }
                else
                {
                    instanceRoot.GetBehaviorDynamic(behaviorClientLocal).Purge();
                }
            }
        }
    }
}