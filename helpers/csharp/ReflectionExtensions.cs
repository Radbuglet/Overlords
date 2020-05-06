using System.Diagnostics;
using System.Reflection;

namespace Overlords.helpers.csharp
{
    public static class ReflectionExtensions
    {
        public static void SetValueSafe(this FieldInfo fieldInfo, object instance, object value)
        {
            Debug.Assert(fieldInfo.FieldType.IsInstanceOfType(value), "Invalid reflection field assignment: unassignable type");
            fieldInfo.SetValue(instance, value);
        }
    }
}