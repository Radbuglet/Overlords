﻿using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
{
    // TODO: Warnings and error checking
    // TODO: CLIENT SAFETY!
    public class ListReplicator: Node, IReplicatorCatchesUp
    {
        private class ReplicatedObject
        {
            public static readonly StructSerializer<ReplicatedObject> Serializer = new StructSerializer<ReplicatedObject>(
                () => new ReplicatedObject(),
                new System.Collections.Generic.Dictionary<string, ISerializerRaw>
                {
                    [nameof(Name)] = new PrimitiveSerializer<string>(),
                    [nameof(EntityTypeId)] = new PrimitiveSerializer<int>()
                });

            public object Serialize()
            {
                return Serializer.Serialize(this);
            }
            
            public string Name;
            public int EntityTypeId;
        }
        
        public delegate bool CanReplicateInstanceTo(int peerId, Node instance);
        
        [Export] private Array<PackedScene> _entityTypes = new Array<PackedScene>();
        private readonly Godot.Collections.Dictionary<string, int> _resourceToPrefabMappings = new Godot.Collections.Dictionary<string, int>();
        public CanReplicateInstanceTo CatchupReplicationValidator;

        public override void _Ready()
        {
            this.RegisterCatchupReceiver();
            var index = 0;
            foreach (var entityType in _entityTypes)
            {
                _resourceToPrefabMappings.Add(entityType.ResourceName, index);
                index++;
            }
        }

        public object SerializeChildren(IEnumerable<Node> targets)
        {
            var replicatedObjects = new Array();
            foreach (var child in targets)
            {
                if (!_resourceToPrefabMappings.TryGetValue(child.Filename, out var prefabIndex))
                {
                    GD.PushWarning($"Child of name \"{child.Name}\" doesn't have a registered prefab type!");
                    continue;
                }

                replicatedObjects.Add(new ReplicatedObject
                {
                    Name = child.Name,
                    EntityTypeId = prefabIndex
                }.Serialize());
            }

            return replicatedObjects;
        }
        
        public IEnumerable<Node> CatchupJoinedPeer(int targetPeerId)
        {
            IEnumerable<Node> EnumerateReplicatedInstances()
            {
                foreach (var childUnCasted in GetChildren())
                {
                    var child = (Node) childUnCasted;
                    if (CatchupReplicationValidator(targetPeerId, child))
                    {
                        yield return child;
                    }
                }
            }
            
            // Replicate
            var pushedObjects = SerializeChildren(EnumerateReplicatedInstances());
            RpcId(targetPeerId, nameof(_PushObjects), pushedObjects);
            
            // Replicated branch culling
            return EnumerateReplicatedInstances();
        }

        public void PushObjectsRemotely(IEnumerable<int> peers, IEnumerable<Node> instances)
        {
            var packet = SerializeChildren(instances);
            foreach (var peerId in peers)
            {
                RpcId(peerId, nameof(_PushObjects), packet);
            }
        }

        public void RemoveObjectsRemotely(IEnumerable<int> peers, Array<string> names)
        {
            foreach (var peerId in peers)
            {
                RpcId(peerId, nameof(_RemoveObjects), names);
            }
        }

        [Puppet]
        private void _PushObjects(object objectsRaw)
        {
            Array<ReplicatedObject> replicatedObjects;
            try
            {
                replicatedObjects = ListSerialization.Deserialize(ReplicatedObject.Serializer, objectsRaw);
            }
            catch (DeserializationException)
            {
                GD.PushWarning($"Failed to deserialize pushed objects in {nameof(ListReplicator)}.");
                return;
            }

            foreach (var replicatedObject in replicatedObjects)
            {
                if (replicatedObject.EntityTypeId < 0 || replicatedObject.EntityTypeId >= _entityTypes.Count)
                {
                    GD.PushWarning($"Failed to deserialize pushed object: invalid {nameof(replicatedObject.EntityTypeId)}.");
                    continue;
                }

                var instance = _entityTypes[replicatedObject.EntityTypeId].Instance();
                instance.Name = replicatedObject.Name;  // TODO: Validate name!
                AddChild(instance);
            }
        }
        
        [Puppet]
        private void _RemoveObjects(object namesRaw)
        {
            Array<string> names;
            try
            {
                names = ListSerialization.Deserialize(new PrimitiveSerializer<string>(), namesRaw);
            }
            catch (DeserializationException)
            {
                GD.PushWarning("Failed to deserialize object removal packet root.");
                return;
            }

            foreach (var name in names)
            {
                var child = GetNodeOrNull<Node>(name);  // TODO: Validate "path"!
                if (child == null)
                {
                    GD.PushWarning("Failed to remove child: child with that name doesn't exist!");
                    continue;
                }
                child.Purge();
            }
        }
        
        [Puppet]
        private void _InsertObject(object newObject, object anchorNodeName, object isBeforeAnchor)
        {
            throw new NotImplementedException();
        }
    }
}