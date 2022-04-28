// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;

namespace DAZ_Installer.DP
{
    /// <summary>
    /// A DPRecord is a record of fully extracted and moved files. Records should be accessed via the Library and after successful extractions.
    /// </summary>

    
    public record DPExtractionRecord(string ArchiveFileName, string DestinationPath, 
                                    string[] Files, string[] ErroredFiles,
                                    string[] ErrorMessages, string[] Folders, uint PID)
    {
        public static readonly DPExtractionRecord NULL_RECORD = new DPExtractionRecord(null, null, null, null, null, null, 0);
    }
}
