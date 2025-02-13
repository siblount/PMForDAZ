using DAZ_Installer.IO;
using Moq;

namespace DAZ_Installer.Windows.DP.Tests {
    internal static class DPExtractJobTestHelpers {
        /// <summary>
        /// Asserts that after the <see cref="DPExtractJob"/> finishes, that for a given archive,
        /// the archive has seen <paramref name="statuses"/> statuses.
        /// </summary>
        /// <param name="extractView">The mock extract view to use</param>
        /// <param name="archive">The path of the archive</param>
        /// <param name="statuses">The expected statuses set for an archive.</param>
        internal static void AssertStatusUpdates(Mock<IExtractView> extractView, string archive, params DPArchiveStatus[] statuses) {
            foreach (var status in statuses) {
                extractView.Verify(x => x.OnExtractJobStatusUpdate(
                    It.IsAny<DPExtractJob>(),
                    It.Is<DPArchiveInfo>(x => MatchArchiveInfoByNameAndStatus(x, archive, status))),
                    Times.AtLeastOnce()
                );
            }
        }

        /// <summary>
        /// Returns whether an <see cref="DPArchiveInfo"/> matches by the <see cref="DPArchiveInfo.FilePath"/> 
        /// and <see cref="DPArchiveInfo.Status"/>.
        /// </summary>
        /// <param name="info">The archive info to compare</param>
        /// <param name="path">The path to compare the <paramref name="info"/> with.</param>
        /// <param name="status">The status to compare the <paramref name="info"/> with</param>
        /// <returns></returns>
        internal static bool MatchArchiveInfoByNameAndStatus(DPArchiveInfo info, string path, DPArchiveStatus status) {
            return PathHelper.NormalizePath(info.FilePath) == PathHelper.NormalizePath(path) && info.Status == status;
        }
    }
}