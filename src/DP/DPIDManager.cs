// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.DP
{
    internal struct DPIDManager
    {
        private static uint lastID = 1;

        /// <summary>
        ///
        /// </summary>
        /// <returns>A unique new tag.</returns>
        internal static uint GetNewID() => lastID++;

    }
}
