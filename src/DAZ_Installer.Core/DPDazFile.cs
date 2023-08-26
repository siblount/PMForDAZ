// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using Serilog;
using DAZ_Installer.IO;

namespace DAZ_Installer.Core
{
    public class DPDazFile : DPFile
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPDazFile>();
        public DPContentInfo ContentInfo = new();
        public DPDazFile(string _path, DPArchive arc, DPFolder? __parent) : base(_path, arc, __parent) => arc.DazFiles.Add(this);

        /// <summary>
        /// Reads and updates <c>ContentInfo</c> struct.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        public void ReadContents(StreamReader stream)
        {
            var tenLines = new string?[10];
            for (var i = 0; i < 10; i++)
                tenLines[i] = stream.ReadLine();
            UpdateContentInfo(tenLines);
        }

        /// <summary>
        /// Parses through the content info and updates the <c>ContentInfo</c> struct. Only reads lines 1 - 8.
        /// </summary>
        /// <param name="contents">A collection of strings.</param>
        public void UpdateContentInfo(ReadOnlySpan<string?> contents)
        {
            // 1..9
            foreach (var line in contents.Slice(1, 9))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    ReadOnlySpan<char> propertyName = GetPropertyName(line);
                    if (propertyName.Contains("type", StringComparison.Ordinal)) ContentInfo.ContentType = GetContentType(ParseJsonValue(line), this);
                    else if (propertyName.Contains("author", StringComparison.Ordinal)) ContentInfo.Authors.Add(ParseJsonValue(line));
                    else if (propertyName.Contains("email", StringComparison.Ordinal)) ContentInfo.Email = ParseJsonValue(line);
                    else if (propertyName.Contains("website", StringComparison.Ordinal)) ContentInfo.Website = ParseJsonValue(line);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to add metadata for file: {0}");
                    // DPCommon.WriteToLog($"Failed to add metadata for file: {RelativePath} REASON: {e}");
                }
            }
        }

        public static string ParseJsonValue(ReadOnlySpan<char> jsonString)
        {
            var colIndex = jsonString.IndexOf(':');
            if (colIndex == -1) return string.Empty;

            ReadOnlySpan<char> afterColString = jsonString.Slice(colIndex + 1);
            var startSearchIndex = afterColString.IndexOf('"');
            var lastQuoteIndex = afterColString.LastIndexOf('"');

            if (startSearchIndex == -1 || lastQuoteIndex == startSearchIndex)
                return string.Empty;

            return afterColString.Slice(startSearchIndex + 1, lastQuoteIndex - startSearchIndex - 1).ToString();
        }

        public static ReadOnlySpan<char> GetPropertyName(ReadOnlySpan<char> msg)
        {
            var colonIndex = msg.IndexOf(':');
            if (colonIndex == -1) return string.Empty;
            ReadOnlySpan<char> propertyName = msg.Slice(0, colonIndex - 1);
            return propertyName.Trim('"').TrimStart();
        }


    }
}