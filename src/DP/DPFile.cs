// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace DAZ_Installer.DP
{
    internal class DPFile : IDPWorkingFile
    {

        // Public static members
        private static Dictionary<string, ContentType> enumPairs { get; } = new Dictionary<string, ContentType>(Enum.GetValues(typeof(ContentType)).Length);
        public static bool initalized = false;
        public static readonly HashSet<string> DAZFormats = new HashSet<string>() { "duf", "dsa", "dse", "daz", "dsf", "duf", "dsb", "dson", "ds", "dsb", "djl", "dsx", "dsi", "dcb", "dbm", "dbc", "dbl", "dso", "dsd", "dsv" };
        public static readonly HashSet<string> GeometryFormats = new HashSet<string>() { "dae", "bvh", "fbx", "obj", "dso", "abc", "mdd", "mi", "u3d" };
        public static readonly HashSet<string> MediaFormats = new HashSet<string>() { "png", "jpg", "hdr", "hdri", "bmp", "gif", "webp", "eps", "raw", "tiff", "tif", "psd", "xcf", "jpeg", "cr2", "svg", "apng", "avif" };
        public static readonly HashSet<string> DocumentFormats = new HashSet<string>() { "txt", "pdf", "doc", "docx", "odt", "html", "ppt", "pptx", "xlsx", "xlsm", "xlsb", "rtf" };
        public static readonly HashSet<string> OtherFormats = new HashSet<string>() { "exe", "lib", "dll", "bat", "cmd" };
        public static readonly HashSet<string> AcceptableImportFormats = new HashSet<string>() { "rar", "zip", "7z" };
        public static Dictionary<string, DPFile> DPFiles = new Dictionary<string, DPFile>();
        // Public members
        public DPFile self;

        // Used for identification
        // TO DO: Add struct for metadata.
        public ContentType contentType;
        public string author;
        public string website;
        public string email;
        public string id;
        //
        public bool errored { get; set; }

        //
        public string path { get; set; }
        public string relativePath { get; set; }
        public string destinationPath { get; set; }
        public string ext { get; set; }
        public bool extract { get; set; } = true;
        public uint uid { get; set; }
        /// <summary>
        /// Parent of current file. When setting parent to a folder, property will call addChild() and handle contents appropriately.
        /// </summary>
        public DPFolder parent
        {
            get => _parent;
            set
            {
                // If we were null, but now we're not...
                if (_parent == null && value != null)
                {
                    // Remove ourselves from root content.
                    try
                    {
                        DPProcessor.workingArchive.rootContents.Remove(this);
                    }
                    catch { }

                    // Call the DPFolder's addchild function to add ourselves to the children list.
                    var s = (IDPWorkingFile)this;
                    value.addChild(ref s);

                    _parent = value;
                }
                else if (_parent == null && value == null)
                {
                    // Find parent.
                    var s = (IDPWorkingFile)this;
                    var potParent = DPProcessor.workingArchive.FindParent(ref s);
                    if (potParent != null)
                    {
#pragma warning disable CA2011 // Avoid infinite recursion
                        parent = potParent; // Recursion will handle _parent setting.

                        // Goes to first if.
                    }
                    else
                    {
                        potParent = DPFolder.CreateFolderForFile(path);
                        if (potParent != null) parent = potParent; // Recursion will handle _parent setting.
                        // Goes to first if.
                        else
                        {
                            _parent = null;
                            if (!DPProcessor.workingArchive.rootContents.Contains(s))
                            {
                                DPProcessor.workingArchive.rootContents.Add(s);
                            }
                        }
                    }
#pragma warning restore CA2011 // Avoid infinite recursion
                }
                else if (_parent != null && value != null)
                {
                    // Remove ourselves from previous parent children.
                    var s = (IDPWorkingFile)this;
                    _parent.removeChild(ref s);

                    // Add ourselves to new parent children.
                    value.addChild(ref s);

                    _parent = value;
                }
                else if (_parent != null && value == null)
                {
                    // Remove ourselves from previous parent children.
                    var s = (IDPWorkingFile)this;
                    _parent.removeChild(ref s);

                    DPProcessor.workingArchive.rootContents.Add(this);
                    _parent = value;
                }
            }
        }
        public ListViewItem associatedListItem { get; set; }
        public TreeNode associatedTreeNode { get; set; }
        public DPArchive associatedArchive { get; set; }
        public DPFolder _parent { get; set; }
        private string listName;
        public bool wasExtracted { get; set; } = false;
        public string extractedPath { get; set; }
        public string ListName { get => listName; set => listName = value; }

        public enum ContentType
        {
            Scene,
            Scene_Subset,
            Hierachical_Material,
            Preset_Hierarchical_Pose,
            Wearable,
            Character,
            Figure,
            Prop,
            Preset_Properties,
            Preset_Shape,
            Preset_Pose,
            Preset_Material,
            Preset_Shader,
            Preset_Camera,
            Preset_Light,
            Preset_Render_Settings,
            Preset_Simulation_Settings,
            Preset_DFormer,
            Preset_Layered_Image,
            Preset_Puppeteer,
            Modifier, // aka morph
            UV_Set,
            Script,
            Library,
            Program,
            Media,
            Document,
            Geometry,
            DAZ_File,
            Unknown
        }

        // TO DO : Add get tags func.
        // TO DO: Add static function to search for a property.
        public static void Initalize()
        {
            initalized = true;
            // Add content types for fast search.
            foreach (var eName in Enum.GetNames(typeof(ContentType)))
            {
                var lowercasedName = eName.ToLower();
                enumPairs[lowercasedName] = (ContentType)Enum.Parse(typeof(ContentType), eName);
            }
        }

        public static ContentType GetContentType(string type, ref DPFile dP)
        {
            if (!string.IsNullOrEmpty(type) && enumPairs.TryGetValue(type, out ContentType contentType))
            {
                return contentType;
            }
            if (dP is null) return ContentType.DAZ_File;
            if (GeometryFormats.Contains(dP.ext))
            {
                return ContentType.Geometry;
            }
            else if (MediaFormats.Contains(dP.ext))
            {
                return ContentType.Media;
            }
            else if (DocumentFormats.Contains(dP.ext))
            {
                return ContentType.Document;
            }
            else if (OtherFormats.Contains(dP.ext))
            {
                return ContentType.Program;
            }
            else if (DAZFormats.Contains(dP.ext))
            {
                return ContentType.DAZ_File;
            }

            // The most obvious comment ever - implied else :\
            return ContentType.Unknown;
        }

        public static string ParseJsonValue(string jsonString, string propertyName)
        {
            // Substring via propertyName length + 6.
            var startSearchIndex = jsonString.IndexOf('"', jsonString.IndexOf(":"));
            var lastQuoteIndex = jsonString.LastIndexOf('"');
            if (startSearchIndex == -1 || lastQuoteIndex == startSearchIndex) return string.Empty;
            var propertyValue = jsonString.Substring(startSearchIndex + 1, lastQuoteIndex - startSearchIndex - 1);

            return propertyValue;

        }

        // Not accurate but it's okay.
        public static string GetPropertyName(string msg) => msg.Remove(msg.IndexOf(':')).Trim('"').TrimStart();
        public void UpdateContentInfo(string[] contents)
        {
            foreach (var line in contents[1..9])
            {
                if (line is null) continue;
                try
                {
                    var propertyName = GetPropertyName(line);
                    if (propertyName.Contains("id")) id = ParseJsonValue(line, "id");
                    else if (propertyName.Contains("type")) contentType = GetContentType(ParseJsonValue(line, "type"), ref self);
                    else if (propertyName.Contains("author")) author = ParseJsonValue(line, "author");
                    else if (propertyName.Contains("email")) email = ParseJsonValue(line, "email");
                    else if (propertyName.Contains("website")) website = ParseJsonValue(line, "website");
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Failed to add metadata for file. REASON: {e}");
                }
            }
        }

        public static bool ValidImportExtension(string ext)
        {
            return ArrayHelper.Contains(AcceptableImportFormats, ext);
        }
        public DPFile()
        {

        }

        public DPFile(string _path, DPFolder __parent)
        {
            uid = DPIDManager.GetNewID();
            DPGlobal.dpObjects.Add(uid, this);
            extract = true;
            path = _path;
            parent = __parent;
            if (path != null | path != "")
            {
                // _ext can have length of 0, ex: LICENSE
                var _ext = Path.GetExtension(path);
                ext = _ext.Length != 0 ? _ext.Substring(1) : string.Empty;
            }
            ListName = DPProcessor.workingArchive.fileName + '\\' + path;
            DPFiles.TryAdd(path, this);
            DPProcessor.workingArchive.contents.Add(this);
        }
        ~DPFile()
        {
            DPFiles.Remove(path);
            DPIDManager.RemoveID(uid);
        }

        public async void QuickReadFileAsync()
        {
            // TO DO: Use GZIP file header check.
            var workingPath = destinationPath != null ? destinationPath : extractedPath;
            try
            {
                using (StreamReader inputFile = new StreamReader(workingPath))
                {
                    // Get first 10 lines.
                    var tenLines = new string[10];
                    for (var i = 0; i < 10; i++)
                    {
                        tenLines[i] = await inputFile.ReadLineAsync();
                    }
                    UpdateContentInfo(tenLines);
                    inputFile.Close();
                    inputFile.Dispose();
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog(e);
                // Try GZIP method.

                try
                {
                    using (GZipStream stream = new GZipStream(new FileStream(workingPath, FileMode.Open), CompressionMode.Decompress))
                    {

                        using (StreamReader gInputFile = new StreamReader(stream))
                        {
                            // Get first 10 lines.
                            var tenLines = new string[10];
                            for (var i = 0; i < 10; i++)
                            {
                                tenLines[i] = await gInputFile.ReadLineAsync();
                            }
                            gInputFile.Dispose();
                            UpdateContentInfo(tenLines);
                        }
                        stream.Close();
                        stream.Dispose();
                    }
                    // Parse data to JSON.

                }
                catch (Exception f)
                {
                    DPCommon.WriteToLog("GZip method failed.");
                    DPCommon.WriteToLog(f);
                    ///errored = true;
                }
            }
        }

        public bool IsReadable()
        {
            var extractPathExists = !wasExtracted && File.Exists(extractedPath);
            var destinationPathExists = wasExtracted && File.Exists(destinationPath);
            var isDazFile = DAZFormats.Contains(ext);
            if (!isDazFile) return false;
            var canRead = false;
            try
            {
                if (extractPathExists)
                    File.Open(extractedPath, FileMode.Open, FileAccess.Read).Dispose();
                else if (destinationPathExists)
                    File.Open(destinationPath, FileMode.Open, FileAccess.Read).Dispose();
                else return false;
                canRead = true;
            }
            catch
            {
                DPCommon.WriteToLog("not readable");
                return false;
            }
            return canRead;
        }

        public static bool FindFileInDPFiles(string path, out DPFile file)
        {
            if (DPFiles.TryGetValue(path, out file)) return true;

            file = null;
            return false;
        }
    }

}
