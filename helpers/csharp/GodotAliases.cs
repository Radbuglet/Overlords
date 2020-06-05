using Godot;

namespace Overlords.helpers.csharp
{
    public static class GodotAliases
    {
        public static Vector3 GetGlobalPosition(this Spatial spatial)
        {
            return spatial.GlobalTransform.origin;
        }
    }
}