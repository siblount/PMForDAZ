using DAZ_Installer.Core;

namespace DAZ_Installer.Windows.Tests {
    public class DPProcessSettingsComparer : IEqualityComparer<DPProcessSettings>
    {
        public static readonly DPProcessSettingsComparer Instance = new();

        private DPProcessSettingsComparer() { }
        public bool Equals(DPProcessSettings x, DPProcessSettings y)
        {
            // Compare simple properties
            if (x.TempPath != y.TempPath) return false;
            if (x.DestinationPath != y.DestinationPath) return false;
            if (x.InstallOption != y.InstallOption) return false;
            if (x.OverwriteFiles != y.OverwriteFiles) return false;

            // Compare ContentFolders (HashSet)
            if (!CompareHashSets(x.ContentFolders, y.ContentFolders)) return false;

            // Compare ContentRedirectFolders (Dictionary)
            if (!CompareDictionaries(x.ContentRedirectFolders, y.ContentRedirectFolders)) return false;

            // Compare ForceFileToDest (Dictionary)
            return CompareDictionaries(x.ForceFileToDest, y.ForceFileToDest);
        }

        private static bool CompareHashSets<T>(HashSet<T>? set1, HashSet<T>? set2)
        {
            if (ReferenceEquals(set1, set2)) return true;
            if (set1 == null || set2 == null) return set1 == set2;
            return set1.SetEquals(set2);
        }

        private static bool CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue>? dict1, Dictionary<TKey, TValue>? dict2) 
            where TKey : notnull
        {
            if (ReferenceEquals(dict1, dict2)) return true;
            if (dict1 == null || dict2 == null) return dict1 == dict2;
            return dict1.Count == dict2.Count && 
                !dict1.Except(dict2).Any();
        }

        public int GetHashCode(DPProcessSettings obj)
        {
            HashCode hash = new();
            hash.Add(obj.TempPath);
            hash.Add(obj.DestinationPath);
            hash.Add(obj.InstallOption);
            hash.Add(obj.OverwriteFiles);
            
            if (obj.ContentFolders != null)
                foreach (var item in obj.ContentFolders.OrderBy(x => x))
                    hash.Add(item);

            if (obj.ContentRedirectFolders != null)
                foreach (var kvp in obj.ContentRedirectFolders.OrderBy(x => x.Key))
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value);
                }

            if (obj.ForceFileToDest != null)
                foreach (var kvp in obj.ForceFileToDest.OrderBy(x => x.Key.ToString()))
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value);
                }

            return hash.ToHashCode();
        }
    }
}