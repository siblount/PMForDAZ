using System;
using IOPath = System.IO.Path;
using System.IO;
using System.Collections.Generic;
namespace DAZ_Installer.DP {
    /// <summary>
    /// A special class that marks the DPFile as .dsx file which typically is a Supplement file or a Manifest file.
    /// </summary>
    internal class DPDSXFile : DPFile {
        internal bool isSupplementFile, isManifestFile = false;
        internal bool isSupportingFile = false;
        internal bool contentChecked = false;
        internal DPContentInfo ContentInfo = new DPContentInfo();
        
        internal DPDSXFile(string _path, DPFolder? __parent) : base(_path, __parent) {
            DPProcessor.workingArchive.DSXFiles.Add(this);
            Tags.Clear(); // Do not include us.
        }

        /// <summary>
        /// Reads the contents of this file and updates the <c>ContentInfo</c> variables.
        /// </summary>
        internal void CheckContents() {
            var parser = new DPDSXParser(ExtractedPath);
            var collection = parser.GetDSXFile();
            var search = collection.FindElementViaTag("ProductName");
            if (search.Length != 0) {
                AssociatedArchive.ProductInfo.ProductName = search[0].attributes["VALUE"];
            }
            search = collection.FindElementViaTag("Artist");
            foreach (var artist in search) {
                ContentInfo.Authors.Add(artist.attributes["VALUE"]);
            }
            search = collection.FindElementViaTag("ProductToken");
            if (search.Length != 0) {
                ContentInfo.ID = search[0].attributes["VALUE"];
                AssociatedArchive.ProductInfo.SKU = ContentInfo.ID;
            }
            contentChecked = true;
        }

        /// <summary>
        /// Opens the manifest file, and returns a map with the keys being the full path of the file and the value being the path without "Content\" included. 
        /// This is only for Daz Product since they require this manifest.
        /// </summary>
        /// <returns>
        /// Returns a dictionary containing files to extract and their destination. 
        /// Key is the file path in the archive, and value is the path relative to Content folder.
        /// </returns>
        internal Dictionary<string, string> GetManifestDestinations() {
            var dict = new Dictionary<string, string>();
            try {
                var parser = new DPDSXParser(ExtractedPath);
                var collection = parser.GetDSXFile();
                var elements = collection.GetAllElements();
                dict.EnsureCapacity(elements.Length);
                foreach (var element in elements) {
                    if (element.attributes.ContainsKey("ACTION") && new string(element.TagName) == "File") {
                        var target = element.attributes["TARGET"];
                        if (target == "Content")
                        {
                            // Get value.
                            ReadOnlySpan<char> filePath = element.attributes["VALUE"];
                            var pathWithoutContent = filePath.Slice(7).TrimStart(PathHelper.GetSeperator(filePath));
                            // dict[filePath.ToString()] = IOPath.Combine(DPProcessor.TempLocation, pathWithoutContent);
                            dict[filePath.ToString()] = pathWithoutContent.ToString();
                        }
                        else if (target == "Application")
                        {
                            DPCommon.WriteToLog("Target was application.");
                        }
                    }
                }
            } catch (Exception ex) {
                DPCommon.WriteToLog($"An unexpected error occurred while attempting to determine destination paths through the manifest. REASON: {ex}");
            }
            return dict;
            
        }

        public static void main(string[] args) {
            var f = new DPDSXFile("", null);
        }
    }
}