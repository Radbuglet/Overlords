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
    }
}