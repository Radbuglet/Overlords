using System;
using System.Diagnostics;
using System.Reflection;

namespace Overlords.helpers.csharp
{
    public static class ReflectionExtensions
    {
        public static FieldInfo GetFieldOrFail(this Type type, string name)
        {
            var field = type.GetField(name);
            Debug.Assert(field != null, $"Field named \"{name}\" doesn't exist on type {type.Name}!");
            return field;
        }
        
        public static object GetValueOrFail(this object instance, string name)
        {
            Debug.Assert(instance != null, "Can't get value on null instance!");
            return instance.GetType().GetFieldOrFail(name).GetValue(instance);
        }
        
        public static void SetValueOrFail(this FieldInfo fieldInfo, object instance, object value)
        {
            Debug.Assert(instance != null, "Can't set value on null instance!");
            fieldInfo.SetValue(instance, value);
        }

        public static void SetValueOrFail(this object instance, string name, object value)
        {
            Debug.Assert(instance != null, "Can't set value on null instance!");
            instance.GetType().GetFieldOrFail(name).SetValueOrFail(instance, value);
        }
    }
}