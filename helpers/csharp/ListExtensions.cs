using System.Collections.Generic;

namespace Overlords.helpers.csharp
{
    public static class ListExtensions
    {
        public static bool HasIndex<T>(this IList<T> list, int index)
        {
            return index < 0 || index >= list.Count;
        }
        
        public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
        {
            if (list.HasIndex(index))
            {
                value = default;
                return false;
            }

            value = list[index];
            return true;
        }
    }
}