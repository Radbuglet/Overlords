using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.helpers.replication
{
    public class ListReplicator : Node, IRequiresCatchup
    {
        private const string RemoteInstanceGroup = "remote_instance";
        
        [Export] private Array<PackedScene> _entityTypes = new Array<PackedScene>();
        private Dictionary<string, int> _fileToTypeMap;
        [Signal] public delegate void OnEntityReplicated(Node instance);
        [Signal] public delegate void OnEntityDeReplicated(Node instance);

        // Event handlers
        public override void _Ready()
        {
            this.FlagRequiresCatchup();
            if (this.GetNetworkMode() != NetworkMode.Server) return;  // No setup for client
            
            // Create a mapping between resource file names and entity types
            var index = 0;
            _fileToTypeMap = new Dictionary<string, int>();
            foreach (var entityType in _entityTypes)
            {
                Debug.Assert(entityType != null);
                _fileToTypeMap[entityType.ResourcePath] = index;
                index++;
            }
        }
        
        // Catchup networking
        public object CatchupOverNetwork(int peerId)
        {
            var packet = new Array();
            foreach (var entity in this.EnumerateChildren())
            {
                if (entity.IsVisibleTo(peerId))
                    packet.Add(new Array {entity.Name, GetTypeId(entity)});
            }
            return packet;
        }

        public void HandleCatchupState(object argsRoot)
        {
            if (!(argsRoot is Array entities))
            {
                GD.PushWarning("Failed to catchup list replicator: packet root is not array");
                return;
            }

            foreach (var entityRaw in entities)
            {
                if (!(
                    entityRaw is Array entity && entity.Count == 2 &&
                    entity[0] is string name && entity[1] is int typeId))
                {
                    GD.PushWarning("Ignored replicated entity whose descriptor was invalid.");
                    continue;
                }

                SpawnRemoteEntity(typeId, name);
            }
        }

        // Server replication API
        private int GetTypeId(Node entity)
        {
            var isRegistered = _fileToTypeMap.TryGetValue(entity.Filename, out var typeId);
            Debug.Assert(isRegistered, "Failed to replicate entity: entity type was never registered.");
            return typeId;
        }

        public void ReplicateEntity(Node entity)
        {
            Debug.Assert(this.GetNetworkMode() == NetworkMode.Server);
            Debug.Assert(entity.IsInsideTree());
            
            // Get entity's type
            var typeId = GetTypeId(entity);
            
            // Replicate it!
            foreach (var peerId in entity.EnumerateNetworkViewers())
            {
                RpcId(peerId, nameof(_EntityAddedRemotely), typeId, entity.Name, entity.GenerateCatchupInfo(peerId));
            }
        }

        public void DeReplicateEntity(Node entity)
        {
            Debug.Assert(entity.IsInsideTree());
            foreach (var peerId in entity.EnumerateNetworkViewers())
            {
                DeReplicateEntityTo(entity, peerId);
            }
        }

        public void ReplicateEntityTo(Node entity, int peerId)
        {
            Debug.Assert(entity.IsInsideTree());
            RpcId(peerId, nameof(_EntityAddedRemotely), GetTypeId(entity), entity.Name, entity.GenerateCatchupInfo(peerId));
        }
        
        public void DeReplicateEntityTo(Node entity, int peerId)
        {
            Debug.Assert(entity.IsInsideTree());
            RpcId(peerId, nameof(_EntityRemovedRemotely), entity.Name);
        }

        // Client replication handlers
        private (Node entity, bool success) SpawnRemoteEntity(int typeIndex, string name)
        {
            if (!_entityTypes.TryGetValue(typeIndex, out var typePrefab))
            {
                GD.PushWarning($"Invalid entity type. Server attempted to spawn entity of type {typeIndex}. Valid types are between 0 and {_entityTypes.Count - 1} inclusive.");
                return (null, false);
            }

            if (name == null)
            {
                GD.PushWarning("Failed to spawn remote entity: name is null!");
                return (null, false);
            }
            
            var entity = typePrefab.Instance();
            entity.Name = name;
            entity.AddToGroup(RemoteInstanceGroup);
            AddChild(entity);
            return (entity, true);
        }
        
        [Puppet]
        private void _EntityAddedRemotely(int typeIndex, string name, Dictionary catchupInfo)
        {
            if (GetTree().DoesAnythingRequireCatchup())
            {
                GD.PushWarning("Failed to remotely spawn entity: tree still contained nodes requiring catchup!");
                return;
            }

            var (entity, success) = SpawnRemoteEntity(typeIndex, name);
            if (!success)
                return;

            if (entity.Name != name)
                GD.PushWarning("Name was invalid and entity name was changed when added into the scene tree. This might cause bugs.");

            // Try to catchup the entity
            if (catchupInfo != null)
            {
                var err = GetTree().ApplyCatchupInfo(catchupInfo);
                if (err == null)
                {
                    EmitSignal(nameof(OnEntityReplicated), entity);
                    return;
                }

                GD.PushWarning($"Failed to apply catchup information: {err.GetMessage()}");
            }
            else
            {
                GD.PushWarning("Failed to apply catchup information: no information provided.");
            }
            
            // Cleanup the entity if the catchup was unsuccessful
            entity.Purge();
        }

        [Puppet]
        private void _EntityRemovedRemotely(string name)
        {
            if (name == null)
            {
                GD.PushWarning("Name of entity to be removed was null!");
                return;
            }
            
            var entity = this.GetChildByName(name);
            if (entity != null && entity.IsInGroup(RemoteInstanceGroup))
            {
                EmitSignal(nameof(OnEntityDeReplicated), entity);
                entity.Purge();
            }
            else
            {
                GD.PushWarning("Failed to remove replicated entity: specified remote entity does not exist.");
            }
        }
    }
}