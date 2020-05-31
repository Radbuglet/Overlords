using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
{
    public class ListReplicator : Node
    {
        public delegate TConstructor EntityStateEmitter<out TConstructor>(int targetPeer, Node root);

        public delegate void RemoteEntityInitializer<in TConstructor>(Node root, Node container,
            TConstructor constructor);

        private readonly Dictionary<string, RegisteredEntityType> _fileToEntityType =
            new Dictionary<string, RegisteredEntityType>();

        private readonly List<RegisteredEntityType> _registeredEntityTypes = new List<RegisteredEntityType>();

        public override void _Ready()
        {
            if (this.GetNetworkMode() == NetworkTypeUtils.NetworkMode.None)
                GD.PushWarning("EntityContainer created in a non-networked scene tree!");
        }

        public void RegisterEntityType<TConstructor>(PackedScene scene, ISerializer<TConstructor> constructorSerializer,
            RemoteEntityInitializer<TConstructor> buildEntity, EntityStateEmitter<TConstructor> makeEntityConstructor)
        {
            Debug.Assert(!_fileToEntityType.ContainsKey(scene.ResourcePath));
            var typeIndex = _registeredEntityTypes.Count;
            var entityType = new RegisteredEntityType
            {
                TypeIndex = typeIndex,
                BuildEntity = entityArgsRaw =>
                {
                    if (!constructorSerializer.TryDeserialize(entityArgsRaw, out var entityArgs, out var error))
                    {
                        GD.PushWarning(
                            $"Failed to construct entity due to constructor deserialization error: {error.Message}");
                        return;
                    }

                    buildEntity(scene.Instance(), this, entityArgs);
                },
                SerializeConstructor = (target, instance) => new AddedEntity
                {
                    TypeIndex = typeIndex,
                    ConstructorArgs = constructorSerializer.Serialize(makeEntityConstructor(target, instance))
                }.Serialize()
            };
            _registeredEntityTypes.Add(entityType);
            _fileToEntityType.Add(scene.ResourcePath, entityType);
        }

        public void SvReplicateInstances(int target, IEnumerable<Node> instances)
        {
            var packet = new Array();
            foreach (var instance in instances)
            {
                if (!_fileToEntityType.TryGetValue(instance.Filename, out var entityType))
                {
                    GD.PushWarning("Failed to replicate instance: type not registered!");
                    continue;
                }

                packet.Add(entityType.SerializeConstructor(target, instance));
            }

            RpcId(target, nameof(_ClInstancesReplicated), packet);
        }

        public void SvReplicateInstances(IEnumerable<int> targets, Func<IEnumerable<Node>> iterateInstances)
        {
            foreach (var target in targets) SvReplicateInstances(target, iterateInstances());
        }

        public void SvReplicateInstance(IEnumerable<int> targets, Node instance)
        {
            foreach (var target in targets) SvReplicateInstances(target, instance.AsEnumerable());
        }

        public void SvDeReplicateInstances(IEnumerable<int> targets, IEnumerable<Node> instances)
        {
            var packet = new Array();
            foreach (var instance in instances) packet.Add(instance.Name);
            this.RpcGeneric(targets, nameof(_ClInstancesDeReplicated), true, packet);
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
                if (!AddedEntity.Serializer.TryDeserializedOrWarn(rawEntity, out var addedEntity))
                    continue;

                if (addedEntity.TypeIndex < 0 || addedEntity.TypeIndex >= _registeredEntityTypes.Count)
                {
                    GD.PushWarning("Invalid entity type index!");
                    continue;
                }

                _registeredEntityTypes[addedEntity.TypeIndex].BuildEntity(addedEntity.ConstructorArgs);
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

        private class AddedEntity
        {
            public static readonly StructSerializer<AddedEntity> Serializer = new StructSerializer<AddedEntity>(
                () => new AddedEntity(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(TypeIndex)] = new PrimitiveSerializer<int>(),
                    [nameof(ConstructorArgs)] = new PrimitiveSerializer<object>()
                });

            public object ConstructorArgs;

            public int TypeIndex;

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }

        private class RegisteredEntityType
        {
            public Action<object> BuildEntity;
            public Func<int, Node, object> SerializeConstructor;
            public int TypeIndex;
        }
    }
}