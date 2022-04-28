// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;

namespace DAZ_Installer.DP
{
    public record DPProductRecord(string ProductName, string[] Tags, string Author, string SKU,
                                    DateTime Time, string ThumbnailPath, uint EID, uint ID)
    {
        public static readonly DPProductRecord NULL_RECORD = new DPProductRecord(null, null, null, null, DateTime.MinValue, null, 0, 0);
    }
}
