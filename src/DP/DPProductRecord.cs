// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;

namespace DAZ_Installer
{
    internal record DPProductRecord(string ProductName, string[] Tags, string[] Directories, DateTime Time, string[] FilesExtracted, string ExpectedExtractionRecordLocation, string ExpectedImageLocation, uint ID)
    {

    }
}
