using DAZ_Installer.IO;
using System.IO.Compression;
using DAZ_Installer.Core.Extraction;
using System.Text;
using Serilog.Context;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Provides tags for <see cref="DPArchive"/>s and updates them in the archive.
    /// </summary>
    internal class DPTagProvider : AbstractTagProvider
    {
        /// <inheritdoc/>
        public override HashSet<string> GetTags(DPArchive arc, DPProcessSettings settings)
        {
            // First is always author.
            // Next is folder names.

            // Read DAZ files to see if we can get some juicy information such as
            // the author, content type, website, email, etc.
            ReadContentFiles(arc, settings);
            // Read the meta files to get the product name(s) and other information.
            ReadMetaFiles(arc, settings);
            var tagsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            tagsSet.EnsureCapacity(arc.GetEstimateTagCount() + 5 +
                (arc.Folders.Count * 2) + ((arc.Contents.Count - arc.Subarchives.Count) * 2));
            foreach (DPDazFile file in arc.DazFiles)
            {
                DPContentInfo contentInfo = file.ContentInfo;
                if (contentInfo.Website.Length != 0) tagsSet.Add(contentInfo.Website);
                if (contentInfo.Email.Length != 0) tagsSet.Add(contentInfo.Email);
                tagsSet.UnionWith(contentInfo.Authors);
            }
            foreach (DPFile content in arc.Contents.Values)
            {
                if (content is DPArchive) continue;
                tagsSet.UnionWith(Path.GetFileNameWithoutExtension(content.FileName).Split(' '));
            }
            foreach (KeyValuePair<string, DPFolder> folder in arc.Folders)
            {
                tagsSet.UnionWith(PathHelper.GetFileName(folder.Key).Split(' '));
            }
            tagsSet.UnionWith(arc.ProductInfo.Authors);
            tagsSet.UnionWith(DPArchive.RegexSplitName(arc.ProductInfo.ProductName));
            if (arc.ProductInfo.SKU.Length != 0) tagsSet.Add(arc.ProductInfo.SKU);
            if (arc.ProductInfo.ProductName.Length != 0) tagsSet.Add(arc.ProductInfo.ProductName);
            return arc.ProductInfo.Tags = tagsSet;
        }

        /// <summary>
        /// Reads files that have the extension .dsf and .duf after it has been extracted. 
        /// </summary>
        private void ReadContentFiles(DPArchive arc, DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                arc!.DazFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, arc);
            if (extractSettings.FilesToExtract.Count > 0) arc.ExtractToTemp(extractSettings);
            Stream? stream = null;
            // Read the contents of the files.
            foreach (DPDazFile file in arc!.DazFiles)
            {
                using (LogContext.PushProperty("File", file.Path))
                // If it did not extract correctly we don't have acces, just skip it.
                if (file.FileInfo is null || !file.FileInfo.Exists)
                {
                    Logger.Error("File does not exist on disk (or does not have access to it).");
                    continue;
                }
                try
                {
                    if (!file.FileInfo!.TryAndFixOpenRead(out stream, out Exception? ex))
                    {
                        Logger.Error(ex, $"Failed to open read stream for file: {file.Path}");
                        continue;
                    }
                    if (stream is null)
                    {
                        Logger.Error($"OpenRead returned successful but also returned null stream, skipping {file.Path}");
                        continue;
                    }
                    if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B)
                    {
                        // It is gzipped compressed.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var gstream = new GZipStream(stream, CompressionMode.Decompress);
                        using var streamReader = new StreamReader(gstream, Encoding.UTF8, true);
                        file.ReadContents(streamReader);
                    }
                    else
                    {
                        // It is normal text.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var streamReader = new StreamReader(stream, Encoding.UTF8, true);
                        file.ReadContents(streamReader);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unable to read contents of {file}");
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        /// <summary>
        /// Reads the files listed in <see cref="DPArchive.DSXFiles"/>.
        /// </summary>
        private void ReadMetaFiles(DPArchive arc, DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                arc!.DazFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, arc);
            arc.ExtractContentsToTemp(extractSettings);
            Stream? stream = null!;
            foreach (DPDSXFile file in arc!.DSXFiles)
            {
                using (LogContext.PushProperty("File", file.Path))
                // If it did not extract correctly we don't have acces, just skip it.
                if (file.FileInfo is null || !file.FileInfo.Exists)
                {
                    Logger.Warning("FileInfo was null or returned does not exist, skipping file to read meta data", file.Path);
                    Logger.Debug("FileInfo is null: {0}, FileInfo exists: {1}", file.FileInfo is null, file?.FileInfo?.Exists);
                    continue;
                }
                try
                {
                    if (!file.FileInfo!.TryAndFixOpenRead(out stream, out Exception? ex))
                    {
                        Logger.Error(ex, $"Failed to open read stream for file for reading meta: {file.Path}");
                        continue;
                    }
                    if (stream is null)
                    {
                        Logger.Error($"OpenRead returned successful but also returned null stream, skipping {file.Path}");
                        continue;
                    }
                    if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B)
                    {
                        // It is gzipped compressed.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var gstream = new GZipStream(stream, CompressionMode.Decompress);
                        using var streamReader = new StreamReader(gstream, Encoding.UTF8, true);
                        file.CheckContents(streamReader);
                    }
                    else
                    {
                        // It is normal text.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var streamReader = new StreamReader(stream, Encoding.UTF8, true);
                        file.CheckContents(streamReader);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unable to read contents of {file.Path}");
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
    }
}
