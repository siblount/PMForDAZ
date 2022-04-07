// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer
{
    /// <summary>
    /// A DPRecord is a record of fully extracted and moved files. Records should be accessed via the Library and after successful extractions.
    /// </summary>
    internal record DPExtractionRecord(string ArchiveFileName, string[] FilesLocation, DateTime Time, int Errors, int FilesExtracted, string[] MissingFiles, string[] ErroredFiles, ArchiveType Type, uint ID)
    {

    }

    // TODO: Update product record.
    internal record DPProductRecord(string ProductName, string[] Tags, string[] Directories, DateTime Time, string[] FilesExtracted, string ExpectedExtractionRecordLocation, string ExpectedImageLocation, uint ID)
    {
        
    }

    // TODO: Create simplified search record.
}
