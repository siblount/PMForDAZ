using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DAZ_Installer.IO;

namespace DAZ_Installer.Windows.DP {
    public class PathComparer : IComparer<string?>, IEqualityComparer<string?>
    {
        public static readonly PathComparer Instance = new PathComparer();
        public int Compare(string? x, string? y)
        {
            if (x is null || y is null)
                return string.Compare(x, y);
            return string.Compare(PathHelper.NormalizePath(x), PathHelper.NormalizePath(y));
        }

        public bool Equals(string? x, string? y)
        {
            if (x is null || y is null)
                return string.Equals(x, y);
            return string.Equals(PathHelper.NormalizePath(x), PathHelper.NormalizePath(y));
        }

        public int GetHashCode([DisallowNull] string? obj)
        {
            return string.GetHashCode(PathHelper.NormalizePath(obj));
        }
    }
}