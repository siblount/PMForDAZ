using DAZ_Installer.Core;
using DAZ_Installer.Windows.DP;

namespace DAZ_Installer.Windows.Tests {
    public class DPArchiveInfoComparer : IEqualityComparer<DPArchiveInfo>
    {
        public static readonly DPArchiveInfoComparer Instance = new();

        private DPArchiveInfoComparer() { }
        public bool Equals(DPArchiveInfo? x, DPArchiveInfo? y) {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return ReferenceEquals(x.Archive, y.Archive)
                   && x.FilePath == y.FilePath
                   && x.Status == y.Status
                   && Enumerable.SequenceEqual(x.Errors, y.Errors);
        }

        public int GetHashCode(DPArchiveInfo? obj) => obj?.GetHashCode() ?? 0;
    }
}