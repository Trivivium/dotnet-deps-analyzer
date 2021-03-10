using System.Collections.Generic;

namespace App.Inspection.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection.Count == 0;
        }
    }
}
