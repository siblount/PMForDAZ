using System.Reflection;
using System.Collections.Generic;

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
