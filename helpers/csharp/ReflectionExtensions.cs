using System.Diagnostics;
using System.Reflection;

namespace Overlords.helpers.csharp
{
    public static class ReflectionExtensions
    {
        public static void SetValueSafe(this FieldInfo fieldInfo, object instance, object value)
        {
            Debug.Assert(fieldInfo.FieldType.IsInstanceOfType(value),
                $"Invalid reflection field assignment: {(value == null ? "null" : value.GetType().Name)} is not assignable to {fieldInfo.FieldType.Name}");
            fieldInfo.SetValue(instance, value);
        }
        
        public static void SetValueSafe(this object instance, string name, object value)
        {
            Debug.Assert(instance != null, "Can't set value on null instance!");
            instance.GetType().GetField(name).SetValue(instance, value);
        }

        public static object GetValueSafe<TObj>(this TObj instance, string name)
        {
            return typeof(TObj).GetField(name).GetValue(instance);  // TODO: Assertions
        }
    }
}