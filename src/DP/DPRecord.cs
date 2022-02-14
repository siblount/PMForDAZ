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
    internal record DPExtractionRecord(string archiveFileName, string[] filesLocation, DateTime time, int errors, int filesExtracted, string[] missingFiles, string[] erroredFiles, ArchiveType type, uint id)
    {

    }

    internal record DPProductRecord(string productName, string[] tags, string[] directories, DateTime time, string[] filesExtracted, string expectedExtractionRecordLocation, string expectedImageLocation, uint id)
    {
        
    }
}
