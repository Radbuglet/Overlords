using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Overlords.helpers.network.serialization;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
{
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

        [Signal] public delegate void ObjectReplicated(Node node);
        
        [Signal] public delegate void ObjectPreDeReplicated(Node node);
        
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
                Debug.Assert(!_resourceToPrefabMappings.ContainsKey(entityType.ResourcePath), "Duplicate entity type in configured entity type list!");
                _resourceToPrefabMappings.Add(entityType.ResourcePath, index);
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
                foreach (var child in this.EnumerateChildren())
                {
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
            List<ReplicatedObject> replicatedObjects;
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
                
                // Check name in order to prevent the assertion in the set_name method causing a crash
                if (replicatedObject.Name.Length == 0)
                {
                    GD.PushWarning("Invalid name for list replicated instance: name is empty!");
                    continue;
                }
                
                // See if the name is valid in terms of character set (only assertion in that method has been dealt with by the code above)
                instance.Name = replicatedObject.Name;
                if (instance.Name != replicatedObject.Name)  // This is actually the method used internally.
                {
                    GD.PushWarning("Invalid name for list replicated instance: name character set invalid!");
                    continue;
                }
                
                // See if the name is not a duplicate as that is the only other source of invalid names here.
                if (this.GetChildByName(instance.Name) != null)
                {
                    GD.PushWarning("Invalid name for list replicated instance: duplicate name!");
                    continue; 
                }

                AddChild(instance);
                EmitSignal(nameof(ObjectReplicated), instance);
            }
        }
        
        [Puppet]
        private void _RemoveObjects(object namesRaw)
        {
            List<string> names;
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
                var child = this.GetChildByName(name);
                if (child == null)
                {
                    GD.PushWarning("Failed to remove child: child with that name doesn't exist!");
                    continue;
                }
                EmitSignal(nameof(ObjectPreDeReplicated), child);
                child.Purge();
            }
        }
    }
}