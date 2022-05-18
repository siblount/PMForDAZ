// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using IOPath = System.IO.Path;
using System.IO;
using System.IO.Compression;
using System.Text;
using System;

namespace DAZ_Installer.DP {
    internal class DPZipArchive : DPAbstractArchive
    {
        internal override bool CanReadWithoutExtracting { get => true; }
        private ZipArchive archive;

        internal DPZipArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase) {
            
        }

        ~DPZipArchive()
        {
            // Release the handle of the zip archive if it isn't null.
            archive?.Dispose();
        }

        #region Override Methods

        internal override void Extract()
        {
            mode = Mode.Extract;
            var max = GetExpectedFilesToExtract();
            var i = 0;
            foreach (var file in archive.Entries) {
                DPFile dpfile = null;
                DPAbstractArchive arc = InternalArchives.Find(a => a.Path == file.FullName);
                if (DPFile.FindFileInDPFiles(file.FullName, out dpfile)) {
                    if (dpfile.WillExtract) ExtractFile(file, dpfile);
                }
                if (arc != null && arc.WillExtract) 
                    ExtractFile(file, arc);
                HandleProgressionZIP(archive, ++i, max);
            }
            HandleProgressionZIP(archive, max, max);
        }

        internal override void Peek()
        {
            archive = ZipFile.OpenRead(IsInnerArchive ? ExtractedPath : Path);
            foreach (var entry in archive.Entries) {
                if (string.IsNullOrEmpty(entry.Name)) {
                    // It is a folder.
                    if (!FolderExists(entry.FullName))
                    {
                        var folder = new DPFolder(entry.FullName, null);
                        folder.AssociatedArchive = this;
                    }

                }
                else if (DPFile.ValidImportExtension(GetExtension(entry.Name))) {
                    var newArchive = CreateNewArchive(entry.FullName, true);
                    newArchive.ParentArchive = this;
                    Contents.Add(newArchive);
                } else {
                    var newFile = DPFile.CreateNewFile(entry.FullName, null);
                    newFile.AssociatedArchive = this;
                }
                TrueArchiveSize += (ulong) Math.Max((long) 0, entry.Length);
            }
        }

        internal override void ReadContentFiles()
        {
            foreach (var file in DazFiles) {
                if (!file.WasExtracted) continue;
                try {
                    using (var stream = new FileStream(file.ExtractedPath, FileMode.Open)) {
                        if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B) {
                            // It is gzipped compressed.
                            stream.Seek(0, SeekOrigin.Begin);
                            using (var gstream = new GZipStream(stream, CompressionMode.Decompress)) {
                                using (var streamReader = new StreamReader(gstream, Encoding.UTF8, true)) {
                                    file.ReadContents(streamReader);
                                }
                            }
                        } else {
                            // It is normal text.
                            stream.Seek(0, SeekOrigin.Begin);
                            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true)) {
                                file.ReadContents(streamReader);
                            }
                        }
                    }

                } catch {}
            }
        }

        internal override void ReadMetaFiles()
        {
            foreach (var file in DSXFiles) {
                var entry = archive.GetEntry(file.Path);
                if (entry == null) continue;
                ExtractFile(entry, file);
                if (file.WasExtracted) {
                    file.CheckContents();
                }
            }
        }

        internal override void ReleaseArchiveHandles()
        {
            archive?.Dispose();
        }

        #endregion

        internal int GetExpectedFilesToExtract() {
            int count = 0;
            foreach (var content in Contents) {
                if (content.WillExtract) count++;
            }
            return count;
        }

        private void ExtractFile(ZipArchiveEntry entry, DPAbstractFile file) {
            string expectedPath = file.TargetPath ?? IOPath.Combine(DPProcessor.TempLocation, entry.Name);
            try {
                try {
                    Directory.CreateDirectory(IOPath.GetDirectoryName(expectedPath));
                } catch {}
                entry.ExtractToFile(expectedPath, DPProcessor.OverwriteFiles == SettingOptions.Yes);
                file.WasExtracted = true;
                file.ExtractedPath = expectedPath;
            } catch (IOException e)
            {
                if (e.Message.StartsWith("The file ") && e.Message.EndsWith("already exists"))
                {
                    DPCommon.WriteToLog("The extracted file already existed but user chose not to overwrite files.");
                }
            } catch (Exception e) {
                DPCommon.WriteToLog($"Unable to extract file: {entry.FullName}. Reason: {e}");
            }
        }

        public void HandleProgressionZIP(ZipArchive sender, int i, int max)
        {
            i = Math.Min(i, max);
            if (ProgressCombo == null) ProgressCombo = new DPProgressCombo();
            var percentComplete = (float)i / max;
            var progress = (int)Math.Floor(percentComplete * 100);
            ProgressCombo.UpdateText($"Extracting files...({progress}%)");
            ProgressCombo.ProgressBar.Value = progress;
        }

    }
}