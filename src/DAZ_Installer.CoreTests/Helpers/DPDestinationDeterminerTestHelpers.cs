using DAZ_Installer.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests.Helpers
{
    internal static class DPDestinationDeterminerTestHelpers
    {
        public static void AssertDestinations(HashSet<DPFile> expected, HashSet<DPFile> actual, string basePath)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Destinations' count are not the same.");
            foreach (var file in expected)
            {
                Assert.IsTrue(actual.Contains(file), "Actual destinations does not contain expected file.");
            }
        }

        /// <summary>
        /// Asserts that all files in the archive have the correct <see cref="DPFile.TargetPath"/>s.
        /// </summary>
        /// <param name="actual"> The actual determined destinations from 
        /// <see cref="DPDestinationDeterminer.DetermineDestinations(DPArchive, DPProcessSettings)"/> 
        /// </param>
        /// <param name="expectedPaths"> The expected paths to be </param>
        public static void AssertTargetPaths(HashSet<DPFile> actual, IEnumerable<string> expectedPaths)
        {
            var expectedHashSet = new HashSet<string>(expectedPaths.Select(x => PathHelper.NormalizePath(x)));
            foreach (var file in actual)
            {
                Assert.IsTrue(expectedHashSet.Contains(PathHelper.NormalizePath(file.TargetPath)), $"File {file.FileName} has incorrect TargetPath.");
            }
        }

        /// <summary>
        /// Asserts that all files in the archive have the correct <see cref="DPAbstractNode.RelativeTargetPath"/>s
        /// and <see cref="DPAbstractNode.RelativePathToContentFolder"/> paths.
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        /// <param name="expectedRTPaths">The expected RelativeTargetPaths.</param>
        /// <param name="expectedRPTCFPaths">The expected RelativePathToContentFolder paths.</param>
        public static void AssertRelativePaths(DPArchive arc, IEnumerable<string> expectedRTPaths, IEnumerable<string> expectedRPTCFPaths)
        {
            var normalizedExpectedRTPaths = expectedRTPaths.Select(x => PathHelper.NormalizePath(x));
            var normalizedExpectedRPTCFPaths = expectedRPTCFPaths.Select(x => PathHelper.NormalizePath(x));
            var foundRTs = new HashSet<string>(arc.Contents.Values.Select(x => x.RelativeTargetPath));
            var foundRPTCFs = new HashSet<string>(arc.Contents.Values.Select(x => x.RelativePathToContentFolder));

            foreach (var path in normalizedExpectedRTPaths)
            {
                Assert.IsTrue(foundRTs.Contains(path), $"Expected RelativeTargetPath {path} is not found.");
            }
            foreach (var path in normalizedExpectedRPTCFPaths)
            {
                Assert.IsTrue(foundRPTCFs.Contains(path), $"Expected RelativePathToContentFolder {path} is not found.");
            }
        }

        /// <summary>
        /// Asserts that all files in the archive have the correct <see cref="DPFolder.IsContentFolder"/>s
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        /// <param name="expectedContentFolderPaths">The expected absolute content folders paths.</param>
        public static void AssertContentFolders(DPArchive arc, IEnumerable<string> expectedContentFolderPaths)
        {
            var expectedContentFolderHashSet = new HashSet<string>(expectedContentFolderPaths.Select(x => PathHelper.NormalizePath(x)));
            var contentFolders = arc.Folders.Values.Where(f => f.IsContentFolder);
            Assert.AreEqual(expectedContentFolderHashSet.Count, contentFolders.Count(), "Content folder count is not the same.");
            foreach (var folder in contentFolders)
            {
                Assert.IsTrue(expectedContentFolderHashSet.Contains(folder.NormalizedPath), $"Folder {folder.NormalizedPath} is not marked as content folder.");
            }
        }


    }
}
