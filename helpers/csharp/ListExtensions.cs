using System.Collections.Generic;

namespace Overlords.helpers.csharp
{
    public static class ListExtensions
    {
        public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
        {
            if (index < 0 || index >= list.Count)
            {
                value = default;
                return false;
            }

            value = list[index];
            return true;
        }
    }
}