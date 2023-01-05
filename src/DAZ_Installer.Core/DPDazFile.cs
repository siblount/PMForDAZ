// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;
using System.IO;

namespace DAZ_Installer.Core {
    internal class DPDazFile : DPFile {
        internal DPContentInfo ContentInfo = new DPContentInfo();
        internal DPDazFile(string _path, DPFolder? __parent) : base(_path, __parent) {
            DPProcessor.workingArchive.DazFiles.Add(this);
        }

        /// <summary>
        /// Reads and updates <c>ContentInfo</c> struct.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        internal void ReadContents(StreamReader stream) {
            var tenLines = new string[10];
            for (var i = 0; i < 10; i++) 
                tenLines[i] = stream.ReadLine();
            UpdateContentInfo(tenLines);
        }

        /// <summary>
        /// Parses through the content info and updates the <c>ContentInfo</c> struct. Only reads lines 1 - 8.
        /// </summary>
        /// <param name="contents">A collection of strings.</param>
        public void UpdateContentInfo(ReadOnlySpan<string> contents)
        {
            // 1..9
            foreach (var line in contents.Slice(1, 8))
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var propertyName = GetPropertyName(line);
                    if (propertyName.Contains("type", StringComparison.Ordinal)) ContentInfo.ContentType = GetContentType(ParseJsonValue(line, "type"), this);
                    else if (propertyName.Contains("author", StringComparison.Ordinal)) ContentInfo.Authors.Add(ParseJsonValue(line, "author"));
                    else if (propertyName.Contains("email", StringComparison.Ordinal)) ContentInfo.Email = ParseJsonValue(line, "email");
                    else if (propertyName.Contains("website", StringComparison.Ordinal)) ContentInfo.Website = ParseJsonValue(line, "website");
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Failed to add metadata for file: {RelativePath} REASON: {e}");
                }
            }
        }

        public static string ParseJsonValue(ReadOnlySpan<char> jsonString, string propertyName)
        {
            // Substring via propertyName length + 6.
            var colIndex = jsonString.IndexOf(':');
            if (colIndex == -1) return string.Empty;

            var afterColString = jsonString.Slice(colIndex+1);
            var startSearchIndex = afterColString.IndexOf('"');
            var lastQuoteIndex = afterColString.LastIndexOf('"');

            if (startSearchIndex == -1 || lastQuoteIndex == startSearchIndex)
                return string.Empty;

            return afterColString.Slice(startSearchIndex + 1, lastQuoteIndex - startSearchIndex - 1).ToString();
        }

        // Not accurate but it's okay.
        // throws error when msg is "", colonIndex returns -1 which results in out of index error.
        public static ReadOnlySpan<char> GetPropertyName(ReadOnlySpan<char> msg) {
            var colonIndex = msg.IndexOf(':');
            var propertyName = msg.Slice(0,colonIndex-1);
            propertyName.Trim('"').TrimStart();
            return propertyName;
        }


    }
}