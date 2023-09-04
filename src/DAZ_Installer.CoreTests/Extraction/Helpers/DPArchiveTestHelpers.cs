using DAZ_Installer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using Moq;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.Core.Extraction;
using Serilog;

namespace DAZ_Installer.CoreTests.Extraction
{
    internal static class DPArchiveTestHelpers
    {
        /// <summary>
        /// The default folders and files to add to an archive.
        /// </summary>
        internal static readonly string[] DefaultContents = new[] { "Contents/", "Contents/a.txt", "b.txt", "Contents/A/c.png" };

        /// <summary>
        /// Asserts whether the contents of the archive are as expected.
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        internal static void AssertDefaultContents(DPArchive arc)
        {
            Assert.AreEqual(3, arc.Contents.Count, "Archive contents count does not match");
            Assert.AreEqual(1, arc.RootFolders.Count, "Archive root folders count does not match");
            Assert.AreEqual(2, arc.Folders.Count, "Archive folders count does not match");
            Assert.AreEqual(1, arc.RootContents.Count, "Archive root contents count does not match");
        }

        /// <summary>
        /// Asserts whether the paths of the entities in the archive are as expected (or whether it was added in successfully).
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        /// <param name="entities">The entities to check.</param>
        public static void AssertExtractorSetPathsCorrectly(DPArchive arc, IEnumerable<string> entities)
        {
            foreach (var entity in entities)
            {
                if (!arc.Contents.ContainsKey(PathHelper.NormalizePath(entity)) && !arc.Folders.ContainsKey(PathHelper.NormalizePath(entity)))
                    Assert.Fail("Extractor did not correctly set entity paths");
            }
        }

        /// <summary>
        /// Sets up peeking events for the extractor and asserts whether they were raised correctly by peeking into the specified archive.
        /// </summary>
        /// <param name="extractor">The extractor to use.</param>
        /// <param name="arc">The archive for <paramref name="extractor"/> to peek into.</param>
        public static void RunAndAssertPeekEvents(DPAbstractExtractor extractor, DPArchive arc)
        {
            bool peeked = false;
            extractor.Peeking += () =>
            {
                if (peeked)
                    Assert.Fail("Peeking event was raised more than once");
                peeked = true;
            };
            bool peekFinished = false;
            extractor.PeekFinished += () =>
            {
                if (peekFinished)
                    Assert.Fail("PeekFinished event was raised more than once");
                peekFinished = true;
            };
            extractor.Peek(arc);
            Assert.IsTrue(peeked, "Peeking event was not raised");
            Assert.IsTrue(peekFinished, "PeekFinished event was not raised");
        }

        /// <summary>
        /// Sets up peeking events for the extractor and asserts whether they were raised correctly by peeking into the specified archive.
        /// </summary>
        /// <param name="extractor">The extractor to use.</param>
        /// <param name="arc">The archive for <paramref name="extractor"/> to peek into.</param>
        public static DPExtractionReport RunAndAssertExtractEvents(DPAbstractExtractor extractor, DPExtractSettings settings, bool toTemp = false)
        {
            bool extracting = false;
            extractor.Extracting += () =>
            {
                if (extracting)
                    Assert.Fail("Extracting event was raised more than once");
                extracting = true;
            };
            bool extractFinished = false;
            extractor.ExtractFinished += () =>
            {
                if (extractFinished)
                    Assert.Fail("ExtractFinished event was raised more than once");
                extractFinished = true;
            };
            var report = toTemp ? extractor.ExtractToTemp(settings) : extractor.Extract(settings);
            Assert.IsTrue(extracting, "Extracting event was not raised");
            Assert.IsTrue(extractFinished, "ExtractFinished event was not raised");
            return report;
        }

        public static void AssertExtractFileInfosCorrectlySet(IEnumerable<DPFile> expectedFilesExtracted)
        {
            foreach (var file in expectedFilesExtracted)
            {
                Assert.IsNotNull(file.FileInfo, $"{file.FileName}'s FileInfo is null, want not null");
                Assert.AreEqual(PathHelper.NormalizePath(file.TargetPath),
                                PathHelper.NormalizePath(file.FileInfo!.Path),
                                $"{file}'s FileInfo's Path does not match TargetPath");
            }
        }

        public static void AssertReport(DPExtractionReport want, DPExtractionReport got)
        {
            CollectionAssert.AreEqual(want.ExtractedFiles, got.ExtractedFiles, "Reports' extracted files are not equal");
            CollectionAssert.AreEqual(want.ErroredFiles.Keys, got.ErroredFiles.Keys, "Reports' errored file keys are not equal");
            Assert.AreEqual(want.Settings, got.Settings, "Reports' settings are not equal");
        }

        public static void SetupTargetPaths(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, arc.FileName, file.FileName);
        }

        public static void SetupTargetPathsForTemp(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, file.FileName);
        }

    }
}
