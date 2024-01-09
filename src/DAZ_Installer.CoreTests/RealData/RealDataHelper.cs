namespace DAZ_Installer.Core.Tests.RealData
{
    internal static class RealDataHelper
    {
        public static List<DPProcessorTestManifest> GetManifests(string manifestDir)
        {
            return Directory.EnumerateFiles(manifestDir, "*.json")
                            .Select(file => DPProcessorTestManifest.FromJson(File.ReadAllText(file)))
                            .ToList();
        }

        public static List<string> GetArchives(string archiveDir) => Directory.EnumerateFiles(archiveDir, "*.zip|*.rar|*.7z|*.001").ToList();

        public static List<DPProcessorTestManifest> GetValidTestCases(string archiveDir, string manifestDir)
        {
            var a = new HashSet<string>(GetArchives(archiveDir));
            var m = GetManifests(manifestDir);
            var t = new List<DPProcessorTestManifest>();

            foreach (var manifest in m)
            {
                if (a.Contains(manifest.ArchiveName)) t.Add(manifest);
            }
            return t;
        }
    }
}
