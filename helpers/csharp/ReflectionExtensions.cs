using System;
using System.Diagnostics;
using System.Reflection;

namespace Overlords.helpers.csharp
{
    public static class ReflectionExtensions
    {
        public static void SetValueSafe(this FieldInfo fieldInfo, object instance, object value)
        {
            Debug.Assert(instance != null, "Can't set value on null instance!");
            fieldInfo.SetValue(instance, value);
        }

        public static FieldInfo GetFieldOrFail(this Type type, string name)
        {
            var field = type.GetField(name);
            Debug.Assert(field != null, $"Field named \"{name}\" doesn't exist on type {type.Name}!");
            return field;
        }

        public static void SetValueSafe(this object instance, string name, object value)
        {
            Debug.Assert(instance != null, "Can't set value on null instance!");
            instance.GetType().GetFieldOrFail(name).SetValueSafe(instance, value);
        }

        public static object GetValueSafe(this object instance, string name)
        {
            Debug.Assert(instance != null, "Can't get value on null instance!");
            return instance.GetType().GetFieldOrFail(name).GetValue(instance);
        }
    }
}