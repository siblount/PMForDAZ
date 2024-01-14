using DAZ_Installer.Core.Extraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        public static List<string> GetArchives(string archiveDir) => Directory.EnumerateFiles(archiveDir)
                                                                              .Where(x => x.EndsWith(".zip")
                                                                                          || x.EndsWith(".rar")
                                                                                          || x.EndsWith(".7z")
                                                                                          || x.EndsWith(".001"))
                                                                              .Select(x => Path.GetFileName(x)).ToList();

        public static List<DPProcessorTestManifest> GetValidTestCases(string archiveDir, string manifestDir)
        {
            try
            {
                var a = new HashSet<string>(GetArchives(archiveDir));
                var m = GetManifests(manifestDir);
                var t = new List<DPProcessorTestManifest>();

                foreach (var manifest in m)
                {
                    if (a.Contains(manifest.ArchiveName)) t.Add(manifest);
                }
                return t;
            } catch { return new(); }

        }

        public static void AssertProcess(List<DPArchiveExitArgs> args, DPProcessorTestManifest manifest)
        {
            foreach (var report in args)
            {
                Assert.IsNotNull(report.Report);
                var h = report.Report.ExtractedFiles.ToDictionary(x => x.Path, x => x.RelativePathToContentFolder);
                var map = manifest.Results.Find(x => x.ArchiveName == report.Archive.FileName).Mappings;
                foreach (var mapping in map)
                {
                    Assert.IsTrue(h.ContainsKey(mapping.Key));
                    // Assert whether the relative target paths are the same.
                    Assert.AreEqual(h[mapping.Key], mapping.Value);
                }
            }
        }
    }
}
