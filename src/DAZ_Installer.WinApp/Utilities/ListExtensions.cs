using System.Collections.Generic;
using System.Reflection;

namespace DAZ_Installer.Utilities
{
    internal static class ListExtensions
    {
        public static T[]? GetInnerArray<T>(this List<T> list)
        {
            return list.GetType()
                       .GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                       .GetValue(list) as T[];
        }
    }
}
