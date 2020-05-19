using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Overlords.helpers.network.replication
{
    public class EntityTypeRegistrar
    {
        public class RegisteredType
        {
            public readonly int Index;
            public readonly PackedScene Scene;

            public RegisteredType(int index, PackedScene scene)
            {
                Index = index;
                Scene = scene;
            }
        }
        
        private readonly Dictionary<string, RegisteredType> _fileToEntity = new Dictionary<string, RegisteredType>();
        private readonly List<RegisteredType> _entityTypes = new List<RegisteredType>();

        public IReadOnlyCollection<RegisteredType> EntityTypes => _entityTypes;

        public void RegisterType(PackedScene scene)
        {
            Debug.Assert(!_fileToEntity.ContainsKey(scene.ResourcePath));
            var type = new RegisteredType(_entityTypes.Count, scene);
            _entityTypes.Add(type);
            _fileToEntity.Add(scene.ResourcePath, type);
        }

        public void RegisterTypes(IEnumerable<PackedScene> scenes)
        {
            foreach (var scene in scenes)
            {
                RegisterType(scene);
            }
        }

        public RegisteredType GetTypeFromNode(Node root)
        {
            var gotEntity = _fileToEntity.TryGetValue(root.Filename, out var type);
            Debug.Assert(gotEntity);
            return type;
        }

        public RegisteredType GetTypeFromIndex(int index)
        {
            return index > 0 && index < _entityTypes.Count ? _entityTypes[index] : null;
        }
    }
}