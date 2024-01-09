using DAZ_Installer.Core;
using System.Collections.Generic;
using System.Text.Json;

public struct DPArchiveMap
{
    public string ArchiveName { get; set; }
    public Dictionary<string, string> Mappings;

    public DPArchiveMap(string archiveName, Dictionary<string, string> mappings)
    {
        ArchiveName = archiveName;
        Mappings = mappings;
    }

}