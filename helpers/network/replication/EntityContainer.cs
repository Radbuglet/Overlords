using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
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
        
        private readonly List<RegisteredEntityType> _registeredEntityTypes = new List<RegisteredEntityType>();

        public override void _Ready()
        {
            if (GetTree().GetNetworkMode() == NetworkUtils.NetworkMode.None)
                GD.PushWarning("EntityContainer created in a non-networked scene tree!");
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
            var packet = new Array();
            foreach (var (instance, type) in instances)
            {
                packet.Add(new AddedEntity
                {
                    TypeIndex = type.TypeIndex,
                    ConstructorArgs = type.MakeEntityConstructor(target, instance)
                }.Serialize());
            }

            RpcId(target, nameof(_ClInstancesReplicated), packet);
        }
        
        public void SvReplicateInstances(IEnumerable<int> targets, Func<IEnumerable<(Node, RegisteredEntityType)>> iterateInstances)
        {
            foreach (var target in targets)
            {
                SvReplicateInstances(target, iterateInstances());
            }
        }

        public void SvReplicateInstance(IEnumerable<int> targets, (Node, RegisteredEntityType) instance)
        {
            foreach (var target in targets)
            {
                SvReplicateInstances(target, instance.AsEnumerable());
            }
        }

        public void SvDeReplicateInstances(IEnumerable<int> targets, IEnumerable<Node> instances)
        {
            var packet = new Array();
            foreach (var instance in instances)
            {
                packet.Add(instance.Name);
            }

            if (targets == null)
                Rpc(nameof(_ClInstancesDeReplicated), packet);
            else
            {
                foreach (var peer in targets)
                {
                    RpcId(peer, nameof(_ClInstancesDeReplicated), packet);
                }
            }
        }

        [Puppet]
        private void _ClInstancesReplicated(object raw)
        {
            if (!(raw is Array entities))
            {
                GD.PushWarning("Invalid replicated instance list!");
                return;
            }
            
            GD.Print($"Received {entities.Count} {(entities.Count == 1 ? "entity" : "entities")}");

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

                _registeredEntityTypes[addedEntity.TypeIndex].InstantiateEntity(addedEntity.ConstructorArgs);
            }
        }
        
        [Puppet]
        private void _ClInstancesDeReplicated(object raw)
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