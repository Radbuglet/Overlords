using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace Overlords.game.world
{
    public class EntityContainer: Node
    {
        private readonly Dictionary<string, Node> _entities = new Dictionary<string, Node>();
        
        public void AddEntity(string id, Node node)
        {
            Debug.Assert(!_entities.ContainsKey(id));
            _entities.Add(id, node);
            node.Name = id;
            AddChild(node);
        }

        public void DestroyEntity(string id, bool immediate)
        {
            var gotEntity = _entities.TryGetValue(id, out var entity);
            Debug.Assert(gotEntity);
            if (immediate) entity.Free(); else entity.QueueFree();
            _entities.Remove(id);
        }

        public T GetEntityOrFallback<T>(string id, T fallback) where T: Node
        {
            return _entities.TryGetValue(id, out var node) && node is T castNode ?
                castNode : fallback;
        }
    }
}