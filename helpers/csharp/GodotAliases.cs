using Godot;

namespace Overlords.helpers.csharp
{
    public static class GodotAliases
    {
        public static void SetGlobalPosition(this Spatial spatial, Vector3 position)
        {
            spatial.GlobalTransform = new Transform(spatial.GlobalTransform.basis, position);
        }
        
        public static Vector3 GetGlobalPosition(this Spatial spatial)
        {
            return spatial.GlobalTransform.origin;
        }

        public static T GetScene<T>(this SceneTree tree) where T : Node
        {
            return (T) tree.CurrentScene;
        }

        public static T GetScene<T>(this Node node) where T : Node
        {
            return node.GetTree().GetScene<T>();
        }

        public static Vector3 WithY(this Vector3 vector, float value)
        {
            return new Vector3(vector.x, value, vector.z);
        }
    }
}