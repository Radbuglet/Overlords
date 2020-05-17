using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.game.world
{
    public class EntityContainer: Node
    {
        private class AddedEntity
        {
            public static readonly StructSerializer<AddedEntity> Serializer = new StructSerializer<AddedEntity>(
                () => new AddedEntity(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(TypeIndex)] = new PrimitiveSerializer<int>(),
                    [nameof(ConstructorArgs)] = new PrimitiveSerializer<object>()
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
            
            public int TypeIndex;
            public object ConstructorArgs;
        }

        public class RegisteredEntityType
        {
            public int TypeIndex;
            public Action<object> InstantiateEntity;
            public EntitySerializer<object> MakeEntityConstructor;
        }
        
        public delegate void EntityCreator<in TConstructor>(TConstructor constructorData);
        public delegate TConstructor EntitySerializer<out TConstructor>(int target, Node node);

        private RemoteEvent _remoteOnEntitiesAdded;
        private RemoteEvent _remoteOnEntitiesRemoved;
        private readonly List<RegisteredEntityType> _registeredEntityTypes = new List<RegisteredEntityType>();

        public override void _Ready()
        {
            _remoteOnEntitiesAdded = new RemoteEvent
            {
                Name = "remoteEntitiesAdded"
            };
            _remoteOnEntitiesRemoved = new RemoteEvent
            {
                Name = "remoteEntitiesRemoved"
            };
            AddChild(_remoteOnEntitiesAdded);
            AddChild(_remoteOnEntitiesRemoved);

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (GetTree().GetNetworkMode())
            {
                case NetworkUtils.NetworkMode.Client:
                    _remoteOnEntitiesAdded.Connect(nameof(RemoteEvent.FiredRemotely), this,
                        nameof(_ClInstancesReplicated));
                    _remoteOnEntitiesRemoved.Connect(nameof(RemoteEvent.FiredRemotely), this,
                        nameof(_ClInstancesDeReplicated));
                    break;
                case NetworkUtils.NetworkMode.None:
                    GD.PushWarning("EntityContainer created in a non-networked scene tree!");
                    break;
            }
        }
        
        public RegisteredEntityType RegisterEntityType<TConstructor>(ISerializer<TConstructor> constructorSerializer,
            EntityCreator<TConstructor> makeEntity, EntitySerializer<TConstructor> makeEntityConstructor)
        {
            var entityType = new RegisteredEntityType
            {
                TypeIndex = _registeredEntityTypes.Count,
                InstantiateEntity = rawConstructor =>
                {
                    TConstructor constructor;
                    try
                    {
                        constructor = constructorSerializer.Deserialize(rawConstructor);
                    }
                    catch (DeserializationException e)
                    {
                        GD.PushWarning($"Failed to create replicated entity: {e.Message}");
                        return;
                    }

                    makeEntity(constructor);
                },
                MakeEntityConstructor = (target, instance) =>
                    constructorSerializer.Serialize(makeEntityConstructor(target, instance))
            };
            _registeredEntityTypes.Add(entityType);
            return entityType;
        }

        public void SvReplicateInstances(int target, IEnumerable<(Node, RegisteredEntityType)> instances)
        {
            var packet = new Godot.Collections.Array();
            foreach (var (instance, type) in instances)
            {
                packet.Add(new AddedEntity
                {
                    TypeIndex = type.TypeIndex,
                    ConstructorArgs = type.MakeEntityConstructor(target, instance)
                }.Serialize());
            }
            _remoteOnEntitiesAdded.FireId(target, packet);
        }

        public void SvDeReplicateInstances(IEnumerable<int> targets, IEnumerable<Node> instances)
        {
            var packet = new Godot.Collections.Array();
            foreach (var instance in instances)
            {
                packet.Add(instance.Name);
            }

            if (targets == null)
                _remoteOnEntitiesRemoved.Fire(packet);
            else
            {
                foreach (var peer in targets)
                {
                    _remoteOnEntitiesRemoved.FireId(peer, packet);
                }
            }
        }

        private void _ClInstancesReplicated(int sender, object raw)
        {
            if (!(raw is Array entities))
            {
                GD.PushWarning("Invalid replicated instance list!");
                return;
            }

            foreach (var rawEntity in entities)
            {
                AddedEntity addedEntity;
                try
                {
                    addedEntity = AddedEntity.Serializer.Deserialize(rawEntity);
                }
                catch (DeserializationException e)
                {
                    GD.PushWarning($"Failed to replicate instance due to invalid {nameof(AddedEntity)} root: {e.Message}");
                    continue;
                }

                if (addedEntity.TypeIndex < 0 || addedEntity.TypeIndex >= _registeredEntityTypes.Count)
                {
                    GD.PushWarning("Invalid entity type index!");
                    continue;
                }

                _registeredEntityTypes[addedEntity.TypeIndex].InstantiateEntity(addedEntity);
                GD.Print("Entity remotely spawned.");
            }
        }
        
        private void _ClInstancesDeReplicated(int sender, object raw)
        {
            if (!(raw is Array entityNames))
            {
                GD.PushWarning("Invalid de-replicated instance list!");
                return;
            }

            foreach (var rawName in entityNames)
            {
                if (!(rawName is string name))
                {
                    GD.PushWarning("Invalid name of de-replicated entity! Ignoring...");
                    continue;
                }

                var node = this.GetChildByName(name);
                if (node == null)
                {
                    GD.PushWarning("Failed to de-replicate child entity: node of that name doesn't exist.");
                    continue;
                }
                node.Purge();
            }
        }
    }
}