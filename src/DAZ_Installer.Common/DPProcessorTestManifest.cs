using DAZ_Installer.Core;
using System.Collections.Generic;
using System.Text.Json;

public struct DPProcessorTestManifest
{
    public DPProcessSettings Settings;
    public string ArchiveName;
    public List<DPArchiveMap> Results;

    public static readonly JsonSerializerOptions options = new()
    {
        IncludeFields = true,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = false,
        IgnoreReadOnlyFields = false,
    };

    public DPProcessorTestManifest(DPProcessSettings settings, string name, List<DPArchiveMap> results)
    {
        Settings = settings;
        ArchiveName = name;
        Results = results;
    }

    public string ToJson() => JsonSerializer.Serialize(this, options);

    public static DPProcessorTestManifest FromJson(string json) => JsonSerializer.Deserialize<DPProcessorTestManifest>(json, options);

    public override string ToString() => ToJson();
}