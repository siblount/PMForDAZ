using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Utilities
{
    internal class EnumerableHelper
    {
        /// <summary>
        /// Returns whether any of the string elements in <paramref name="array"/>
        /// contain text from <paramref name="obj"/>. Or in other words; <para/>
        /// for every string in array, if array[i] contains obj, then return true.
        /// Otherwise, false.
        /// </summary>
        /// <param name="array">The array to check strings against.</param>
        /// <param name="obj">The string to compare with.</param>
        /// <returns>Whether any of the strings contain <paramref name="obj"/> string.</returns>
        public static bool StrContains(IEnumerable<string> array, string obj)
        {
            foreach (var _obj in array)
            {
                if (obj.IndexOf(_obj) != -1) return true;
            }
            return false;
        }
    }
}
