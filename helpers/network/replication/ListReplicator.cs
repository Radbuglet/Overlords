using System.Collections.Generic;
using Godot;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
{
    public class ListReplicator : Node
    {
        public delegate void EntityBuilder<in T>(T constructor);

        [Export] public bool AcceptingDynamicInstances;
        private readonly Dictionary<int, EntityBuilder<object>> _entityBuilders = new Dictionary<int, EntityBuilder<object>>();

        public override void _Ready()
        {
            if (this.GetNetworkMode() == NetworkTypeUtils.NetworkMode.None)
                GD.PushWarning("EntityContainer created in a non-networked scene tree!");
        }

        public Array SvSerializeEntities(IEnumerable<(int typeId, object constructor)> instances)
        {
            var packet = new Array();
            foreach (var (typeId, constructor) in instances)
            {
                packet.Add(new AddedEntity
                {
                    TypeIndex = typeId,
                    ConstructorArgs = constructor
                }.Serialize());
            }

            return packet;
        }

        public void SvReplicateEntities(IEnumerable<int> targets, IEnumerable<(int typeId, object constructor)> instances)
        {
            var packet = SvSerializeEntities(instances);
            foreach (var target in targets)
            {
                RpcId(target, nameof(_ClInstancesReplicated), packet);
            }
        }
        
        public void SvDeReplicateInstances(IEnumerable<int> targets, IEnumerable<Node> instances)
        {
            var packet = new Array();
            foreach (var instance in instances) packet.Add(instance.Name);
            this.RpcGeneric(targets, nameof(_ClInstancesDeReplicated), true, packet);
        }

        public void ClRegisterBuilder(int typeId, EntityBuilder<object> builder)
        {
            _entityBuilders.Add(typeId, builder);
        }

        public void ClRegisterBuilder<T>(int typeId, ISerializer<T> serializer, EntityBuilder<T> builder)
        {
            ClRegisterBuilder(typeId, raw =>
            {
                if (!serializer.TryDeserializedOrWarn(raw, out var constructor)) return;
                builder(constructor);
            });
        }

        public void ClManuallyCatchupInstances(Array entities)
        {
            foreach (var rawEntity in entities)
            {
                if (!AddedEntity.Serializer.TryDeserializedOrWarn(rawEntity, out var addedEntity))
                    continue;

                if (addedEntity.TypeIndex < 0 || addedEntity.TypeIndex >= _entityBuilders.Count)
                {
                    GD.PushWarning("Invalid entity type index!");
                    continue;
                }

                _entityBuilders[addedEntity.TypeIndex](addedEntity.ConstructorArgs);
            }
        }

        [Puppet]
        private void _ClInstancesReplicated(object raw)
        {
            if (AcceptingDynamicInstances)
            {
                GD.PushWarning($"Server sent instances but {nameof(AcceptingDynamicInstances)} is not true.");
                return;
            }
            if (!(raw is Array entities))
            {
                GD.PushWarning("Invalid replicated instance list!");
                return;
            }

            GD.Print($"Received {entities.Count} {(entities.Count == 1 ? "entity" : "entities")}");
            ClManuallyCatchupInstances(entities);
        }

        [Puppet]
        private void _ClInstancesDeReplicated(object raw)
        {
            if (AcceptingDynamicInstances)
            {
                GD.PushWarning($"Server de-replicated instances but {nameof(AcceptingDynamicInstances)} is not true.");
                return;
            }
            
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

            public int TypeIndex;
            public object ConstructorArgs;

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
        }
    }
}