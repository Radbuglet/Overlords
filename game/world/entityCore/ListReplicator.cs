﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Overlords.game.definitions;
using Overlords.helpers.csharp;
using Overlords.helpers.network;
using Overlords.helpers.tree;

namespace Overlords.game.world.entityCore
{
    public class ListReplicator : Node, IRequiresCatchup
    {
        [Export] private Array<PackedScene> _entityTypes = new Array<PackedScene>();
        private Godot.Collections.Dictionary<string, int> _fileToTypeMap;

        public override void _Ready()
        {
            if (this.GetNetworkMode() != NetworkMode.Server) return;  // No setup for client
            
            // Create a mapping between resource file names and entity types
            var index = 0;
            _fileToTypeMap = new Godot.Collections.Dictionary<string, int>();
            foreach (var entityType in _entityTypes)
            {
                Debug.Assert(entityType != null);
                _fileToTypeMap[entityType.ResourcePath] = index;
                index++;
            }
        }

        private void ReplicateEntity(Node entity, IEnumerable<int> peerIds)
        {
            Debug.Assert(this.GetNetworkMode() == NetworkMode.Server);
            
            // Get entity's type
            var isRegistered = _fileToTypeMap.TryGetValue(entity.Filename, out var typeId);
            Debug.Assert(isRegistered, "Failed to replicate entity: entity type was never registered.");
            
            // Replicate it!
            foreach (var peerId in peerIds)
            {
                RpcId(peerId, nameof(_EntityAddedRemotely), typeId, entity.Name);
                entity.CatchupToPeer(peerId);
            }
        }

        public void ReplicateEntity(Node entity)
        {
            ReplicateEntity(entity, GetTree().GetPlayingPeers());
        }

        public void DeReplicateEntity(Node entity)
        {
            foreach (var peerId in GetTree().GetPlayingPeers())
            {
                RpcId(peerId, nameof(_EntityRemovedRemotely), entity.Name);
            }
        }
        
        public void CatchupState(int peerId)
        {
            foreach (var entity in GetChildren().Cast<Node>())
            {
                ReplicateEntity(entity, peerId.AsEnumerable());
            }
        }
        
        [Puppet]
        private void _EntityAddedRemotely(int typeIndex, string name)
        {
            if (!_entityTypes.TryGetValue(typeIndex, out var typePrefab))
            {
                GD.PushWarning($"Invalid entity type. Server attempted to spawn entity of type {typeIndex}. Valid types are between 0 and {_entityTypes.Count - 1} inclusive.");
                return;
            }
            var entity = typePrefab.Instance();
            entity.Name = name;
            AddChild(entity);
            if (entity.Name != name)
            {
                GD.PushWarning("Name was invalid and entity name was changed when added into the scene tree. This might cause bugs.");
            }
        }
        
        [Puppet]
        private void _EntityRemovedRemotely(string name)
        {
            if (name == null)
            {
                GD.PushWarning("Name of entity to be removed was null!");
                return;
            }
            
            // While this code seems inefficient when compared to Godot's internal implementation of node getters,
            // this is in fact what Godot does internally.
            foreach (var child in GetChildren().Cast<Node>())
            {
                // ReSharper disable once InvertIf
                if (child.Name == name)
                {
                    child.Purge();
                    return;
                }
            }
            
            GD.PushWarning("Failed to delete entity with that name because it doesn't exist!");
        }
    }
}