// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.IO;
using Serilog;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A special class that marks the DPFile as .dsx file which typically is a Supplement file or a Manifest file.
    /// </summary>
    public class DPDSXFile : DPFile
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPDSXFile>();
        public bool isSupplementFile, isManifestFile = false;
        public bool isSupportingFile = false;
        public bool contentChecked = false;
        public DPContentInfo ContentInfo = new();

        public DPDSXFile(string _path, DPArchive arc, DPFolder? __parent) :
            base(_path, arc, __parent)
        {
            arc.DSXFiles.Add(this);
            if (FileName.Contains("Manifest.dsx")) arc.ManifestFiles.Add(this);
            Tags.Clear(); // Do not include us.
        }

        /// <summary>
        /// Reads the contents of this file and updates the <c>ContentInfo</c> variables.
        /// </summary>
        public void CheckContents(StreamReader stream)
        {
            if (AssociatedArchive is null) return;
            if (!Extracted) throw new InvalidOperationException("Cannot check contents of a file that has not been extracted.");
            if (FileName.EndsWith("Manifest.dsx") || FileName.EndsWith("Supplement.dsx")) return;
            var parser = new DPDSXParser(stream);
            DPDSXElementCollection collection = parser.GetDSXFile();
            List<DPDSXElement> search = collection.FindElementViaTag("ProductName");
            if (search.Count != 0)
            {
                AssociatedArchive.ProductInfo.ProductName = search[0].attributes["VALUE"];
            }
            search = collection.FindElementViaTag("Artist");
            foreach (DPDSXElement artist in search)
            {
                ContentInfo.Authors.Add(artist.attributes["VALUE"]);
                AssociatedArchive.ProductInfo.Authors.Add(artist.attributes["VALUE"]);
            }
            search = collection.FindElementViaTag("ProductToken");
            if (search.Count != 0)
            {
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
        public Dictionary<string, string> GetManifestDestinations()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                if (!Extracted)
                {
                    Logger.Error("Cannot get manifest destinations for a file that has not been extracted.");
                    return dict;
                }
                if (FileInfo is null) { 
                    Logger.Error("FileInfo is null. Cannot get manifest destinations");
                    return dict;
                }
                if (!FileInfo.TryAndFixOpenRead(out var stream, out var ex))
                {
                    Logger.Error(ex, "Failed to open file for reading");
                    return dict;
                }
                using var streamReader = new StreamReader(stream!);
                var parser = new DPDSXParser(streamReader);
                DPDSXElementCollection collection = parser.GetDSXFile();
                IEnumerable<DPDSXElement> elements = collection.GetAllElements();
                dict.EnsureCapacity(collection.Count);
                foreach (DPDSXElement element in elements)
                {
                    if (element.attributes.ContainsKey("ACTION") && new string(element.TagName) == "File")
                    {
                        var target = element.attributes["TARGET"];
                        if (target == "Content")
                        {
                            // Get value.
                            ReadOnlySpan<char> filePath = element.attributes["VALUE"];
                            ReadOnlySpan<char> pathWithoutContent = filePath.Slice(7).TrimStart(PathHelper.GetSeperator(filePath));
                            dict[filePath.ToString()] = pathWithoutContent.ToString();
                        }
                        else if (target == "Application")
                        {
                            Logger.Warning("Got a target where the value was Application. This is not supported yet.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while attempting to determine destination paths through the manifest.");
            }
            return dict;

        }
    }
}