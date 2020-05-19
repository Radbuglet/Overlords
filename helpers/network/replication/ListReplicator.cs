using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Overlords.helpers.csharp;
using Overlords.helpers.tree;
using Array = Godot.Collections.Array;

namespace Overlords.helpers.network.replication
{
    public class ListReplicator: Node
    {
        public delegate object InstanceSerializer(int target, Node instanceRoot);
        public delegate void InstanceBuilder(object serializedInstance);
        
        public InstanceSerializer SerializeInstance;
        public InstanceBuilder BuildRemoteInstance;
        
        public override void _Ready()
        {
            if (GetTree().GetNetworkMode() == NetworkUtils.NetworkMode.None)
                GD.PushWarning("EntityContainer created in a non-networked scene tree!");
        }

        public void SvReplicateInstances(int target, IEnumerable<Node> instances)
        {
            var packet = new Array();
            Debug.Assert(instances != null);
            Debug.Assert(SerializeInstance != null);
            foreach (var instance in instances)
            {
                packet.Add(SerializeInstance(target, instance));
            }

            RpcId(target, nameof(_ClInstancesReplicated), packet);
        }
        
        public void SvReplicateInstances(IEnumerable<int> targets, Func<IEnumerable<Node>> iterateInstances)
        {
            Debug.Assert(targets != null);
            foreach (var target in targets)
            {
                SvReplicateInstances(target, iterateInstances());
            }
        }

        public void SvReplicateInstance(IEnumerable<int> targets, Node instance)
        {
            Debug.Assert(targets != null);
            foreach (var target in targets)
            {
                SvReplicateInstances(target, instance.AsEnumerable());
            }
        }

        public void SvDeReplicateInstances(IEnumerable<int> targets, IEnumerable<Node> instances)
        {
            Debug.Assert(instances != null);
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
            if (BuildRemoteInstance == null)
            {
                GD.PushWarning($"{nameof(BuildRemoteInstance)} is null!");
                return;
            }
            
            foreach (var entity in entities)
            {
                BuildRemoteInstance(entity);
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