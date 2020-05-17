using System;
using System.Collections.Generic;
using Godot;
using Overlords.helpers.network;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree.conditionals;

namespace Overlords.game.world.entityContainer
{
    public class EntityContainer: MultiNetworkNode<EntityContainerServer, EntityContainerClient, Node>
    {
        public class AddedEntity
        {
            public static StructSerializer<AddedEntity> Serializer = new StructSerializer<AddedEntity>(
                () => new AddedEntity(),
                new Dictionary<string, ISerializerRaw>
                {
                    [nameof(NetworkName)] = new PrimitiveSerializer<string>(),
                    [nameof(EntityType)] = new PrimitiveSerializer<int>(),
                    [nameof(EntityArguments)] = new PrimitiveSerializer<object>()
                });
            
            public string NetworkName;
            public int EntityType;
            public object EntityArguments;
        }
        
        private class RegisteredEntityType
        {
            
        }

        public RemoteEventTyped<List<AddedEntity>> OnEntitiesAdded;
        public RemoteEventTyped<List<string>> OnEntitiesRemoved;
        private readonly List<RegisteredEntityType> _registeredEntityTypes = new List<RegisteredEntityType>();
        private readonly Dictionary<string, RegisteredEntityType> _fileToEntityType = new Dictionary<string, RegisteredEntityType>();

        public delegate void EntityCreator<in TCtor>(string networkName, TCtor ctorData);
        
        public void RegisterEntityType<TCtor>(PackedScene prefab, ISerializer<TCtor> serializer, EntityCreator<TCtor> entityCreator)
        {
            throw new NotImplementedException();
        }
        
        protected override void InitializeCommon()
        {
            OnEntitiesAdded = RemoteEventTyped<List<AddedEntity>>.Attach(this, nameof(OnEntitiesAdded),
                new ListSerializer<AddedEntity>(AddedEntity.Serializer));
            
            OnEntitiesRemoved = RemoteEventTyped<List<string>>.Attach(this, nameof(OnEntitiesRemoved),
                new ListSerializer<string>(new PrimitiveSerializer<string>()));
        }

        protected override EntityContainerServer MakeServer()
        {
            return new EntityContainerServer();
        }

        protected override EntityContainerClient MakeClient()
        {
            return new EntityContainerClient();
        }
    }
}