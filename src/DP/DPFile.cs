// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using IOPath = System.IO.Path;
using System.IO.Compression;

namespace DAZ_Installer.DP
{
    internal class DPFile : DPAbstractFile
    {

        // Public static members
        private static Dictionary<string, ContentType> enumPairs { get; } = new Dictionary<string, ContentType>(Enum.GetValues(typeof(ContentType)).Length);
        public static readonly HashSet<string> DAZFormats = new HashSet<string>() { "duf", "dsa", "dse", "daz", "dsf", "dsb", "dson", "ds", "dsb", "djl", "dsx", "dsi", "dcb", "dbm", "dbc", "dbl", "dso", "dsd", "dsv" };
        public static readonly HashSet<string> GeometryFormats = new HashSet<string>() { "dae", "bvh", "fbx", "obj", "dso", "abc", "mdd", "mi", "u3d" };
        public static readonly HashSet<string> MediaFormats = new HashSet<string>() { "png", "jpg", "hdr", "hdri", "bmp", "gif", "webp", "eps", "raw", "tiff", "tif", "psd", "xcf", "jpeg", "cr2", "svg", "apng", "avif" };
        public static readonly HashSet<string> DocumentFormats = new HashSet<string>() { "txt", "pdf", "doc", "docx", "odt", "html", "ppt", "pptx", "xlsx", "xlsm", "xlsb", "rtf" };
        public static readonly HashSet<string> OtherFormats = new HashSet<string>() { "exe", "lib", "dll", "bat", "cmd" };
        public static readonly HashSet<string> AcceptableImportFormats = new HashSet<string>() { "rar", "zip", "7z" };
        public static Dictionary<string, DPFile> DPFiles = new Dictionary<string, DPFile>();

        // Used for identification
        // TO DO: Add struct for metadata.

        internal List<string> Tags { get; set; }
        /// <summary>
        /// Parent of current file. When setting parent to a folder, property will call addChild() and handle contents appropriately.
        /// </summary>
        internal string ListName { get; set; }
        

        // TO DO : Add get tags func.
        // TO DO: Add static function to search for a property.
        static DPFile() {
            foreach (var eName in Enum.GetNames(typeof(ContentType)))
            {
                var lowercasedName = eName.ToLower();
                enumPairs[lowercasedName] = (ContentType)Enum.Parse(typeof(ContentType), eName);
            }
        }

        public DPFile(string _path, DPFolder __parent) : base(_path)
        {
            WillExtract = true;
            Parent = __parent;
            if (Path != null | Path != "")
            {
                // _ext can have length of 0, ex: LICENSE
                var _ext = IOPath.GetExtension(Path);
                Ext = _ext.Length != 0 ? _ext.Substring(1) : string.Empty;
            }
            ListName = DPProcessor.workingArchive.FileName + '\\' + Path;
            DPFiles.TryAdd(Path, this);
            DPProcessor.workingArchive.Contents.Add(this);

            InitializeTagsList();
        }

        internal static DPFile CreateNewFile(string path, DPFolder? parent) {
            var ext = IOPath.GetExtension(path).ToLower();
            if (ext.IndexOf('.') != -1) ext = ext.Substring(1);
            if (ext == "dsf" || ext == "duf") {
                return new DPDazFile(path, parent);
            } else if (ext == "dsx") {
                return new DPDSXFile(path, parent);
            }
            return new DPFile(path, parent);
        }



        public static ContentType GetContentType(string type, DPFile dP)
        {
            if (!string.IsNullOrEmpty(type) && enumPairs.TryGetValue(type, out ContentType contentType))
            {
                return contentType;
            }
            if (dP is null) return ContentType.DAZ_File;
            if (GeometryFormats.Contains(dP.Ext))
            {
                return ContentType.Geometry;
            }
            else if (MediaFormats.Contains(dP.Ext))
            {
                return ContentType.Media;
            }
            else if (DocumentFormats.Contains(dP.Ext))
            {
                return ContentType.Document;
            }
            else if (OtherFormats.Contains(dP.Ext))
            {
                return ContentType.Program;
            }
            else if (DAZFormats.Contains(dP.Ext))
            {
                return ContentType.DAZ_File;
            }

            // The most obvious comment ever - implied else :\
            return ContentType.Unknown;
        }
        

        public static bool ValidImportExtension(string ext) => AcceptableImportFormats.Contains(ext);

        /// <summary>
        /// Adds the file name to the tags name.
        /// </summary>
        protected void InitializeTagsList() {
            var fileName = IOPath.GetFileName(Path);
            var tokens = fileName.Split(' ');
            Tags = new List<string>(tokens.Length);
            Tags.AddRange(tokens);
        }

        public static bool FindFileInDPFiles(string path, out DPFile file)
        {
            if (DPFiles.TryGetValue(path, out file)) return true;

            file = null;
            return false;
        }

        ~DPFile()
        {
            DPFiles.Remove(Path);
        }


    }

}
