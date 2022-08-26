// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using DAZ_Installer.External;
using IOPath = System.IO.Path;
using System.IO;

namespace DAZ_Installer.DP
{
    // ZIP Transversal Check
    /*
        destFileName = Path.GetFullPath(Path.Combine(destDirectory, entry.Key));
        string fullDestDirPath = Path.GetFullPath(destDirectory + Path.DirectorySeparatorChar);
        if (!destFileName.StartsWith(fullDestDirPath)) {
            throw new ExtractionException("Entry is outside of the target dir: " + destFileName);
        }
    */
    // TO DO: Add tag property.

    
    internal class DPRARArchive : DPAbstractArchive
    {
        public bool passwordFailed = false;
        public bool cancelledOperation = false;
        public bool secondPasswordPromptHasSeen = false;
        private List<string> lastVolumes { get; } = new List<string>();
        private Dictionary<string, string> volumePairs = new Dictionary<string, string>(); // First key is the OLD nonworking one, Second key is the working one.

        internal override bool CanReadWithoutExtracting { get => false; }
        protected char[] password;

        protected char internalDictSeperator = '\\';

        public DPRARArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase)
        {

        }
        #region Event Methods

        public void HandleProgression(RAR sender, ExtractionProgressEventArgs e) {
            var progress = (int)Math.Floor(e.PercentComplete);
            ProgressCombo.UpdateText($"Extracting {e.FileName}...({progress}%)");
            ProgressCombo.ProgressBar.Value = progress;
            
        }

        // Will probably remove for safety.
        public void HandlePasswordProtected(RAR sender, PasswordRequiredEventArgs e) {
            var passDlg = new PasswordInput();
            passDlg.archiveName = IOPath.GetFileName(sender.ArchivePathName);
            if (sender.CurrentFile == null) {
                passDlg.message = $"{IOPath.GetFileName(Path)} is encrypted. Please enter password to decrypt archive.";
            } else {
                if ((sender.arcData.Flags & 0x0080) != 0) {
                    passDlg.message = "Password was incorrect. Please re-enter password to decrypt archive.";
                }
            }
            passDlg.ShowDialog();

            if (passDlg.password != null) {
                e.Password = passDlg.password;
                e.ContinueOperation = true;
            } else {
                e.ContinueOperation = false;
                cancelledOperation = true;
            }
        }

        public void RARHandlePassword(RAR sender, PasswordRequiredEventArgs e)
        {
            var password = GetPassword();
            if (string.IsNullOrEmpty(password))
            {
                HandlePasswordProtected(sender, e);
                return;
            }
            e.Password = GetPassword();
            e.ContinueOperation = !cancelledOperation;
            secondPasswordPromptHasSeen = true;
        }

        public void RARHandleVolumes(RAR _, MissingVolumeEventArgs e)
        {
            e.VolumeName = GetRightVolume(e.VolumeName);
        }

        public void HandleNewVolume(RAR sender, NewVolumeEventArgs e)
        {
            if (sender.ArchivePathName != e.VolumeName) ConnectVolumeDir(e.VolumeName);
            if (DPExtractJob.workingJob.doNotProcess.Contains(IOPath.GetFileName(sender.ArchivePathName)))
            {
                DPExtractJob.workingJob.doNotProcess.Add(IOPath.GetFileName(sender.ArchivePathName));
            }
        }

        private void HandleMissingVolume(RAR sender, MissingVolumeEventArgs e)
        {
            var result = MessageBox.Show($"{sender.CurrentFile.FileName} is missing volume : {e.VolumeName}. Do you know where this file is? ", 
                "Missing volume", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) {
                string fileName = MainForm.activeForm.ShowFileDialog("RAR files (*.rar)|*.rar", "rar");
                if (fileName != null)
                {
                    e.VolumeName = fileName;
                    e.ContinueOperation = true;
                    return;
                }
            }
            e.ContinueOperation = false;
        }


        public void HandleNewFile(RAR sender, NewFileEventArgs e)
        {
            DPCommon.WriteToLog(e.fileInfo.FileName);
            if (e.fileInfo.IsDirectory)
            {
                if (!FolderExists(e.fileInfo.FileName))
                {
                    _ = new DPFolder(e.fileInfo.FileName, null);
                    //newDir.parent = workingArchive.FindParent(ref IDP);
                    //if (newDir.parent == null) workingArchive.rootFolders.Add(newDir);
                }

            }
            else
            {
                if (DPFile.ValidImportExtension(GetExtension(e.fileInfo.FileName)))
                {
                    // File is archive.
                    var newArchive = CreateNewArchive(e.fileInfo.FileName, true, null);
                    newArchive.ParentArchive = this;
                }
                else
                {
                    var newFile = DPFile.CreateNewFile(e.fileInfo.FileName, null);
                    newFile.AssociatedArchive = this;
                }
            }
        }


        public void ConnectVolumeDir(string dirPath)
        {
            lastVolumes.Add(IOPath.GetFileName(dirPath));
            HierachyName = FileName + " (";
            foreach (var volume in lastVolumes)
            {
                HierachyName += $"{volume}/";
            }
            HierachyName = HierachyName.TrimEnd('/') + ')';
        }

        public void SetPassword(string pass)
        {
            password = pass.ToCharArray();
        }

        public string GetPassword()
        {
            return new string(password);
        }

        public void AddVolumePair(string expectedVolume, string rightVolume)
        {
            volumePairs.Add(expectedVolume, rightVolume);
        }

        public string GetRightVolume(string expectedVolume)
        {
            if (volumePairs.TryGetValue(expectedVolume, out string rightVolume))
            {
                return rightVolume;
            }
            return null;
        }
        #endregion
        #region Override Methods

        internal override void ReadContentFiles()
        {
            // At this point, the files should have been extracted.
            foreach (var file in DazFiles) {
                // We only want daz files that successfully extracted.
                if (!file.WasExtracted) continue;
                try {
                    using (var reader = new StreamReader(file.ExtractedPath, Encoding.UTF8, true)) {
                        file.ReadContents(reader);
                    }
                } catch {}
            }
        }

        internal override void ReadMetaFiles()
        {
            using (RAR handler = new RAR(IsInnerArchive ? ExtractedPath : Path))
            {
                foreach (var file in DSXFiles)
                {
                    // Extract the file and update the product info and content info structs.
                    if (ExtractFile(handler))
                    {
                        file.CheckContents();
                    }
                }
            }
        }
        internal override void Extract() {
            mode = Mode.Extract;
            ProgressCombo ??= new DPProgressCombo();
            using (var RARHandler = new RAR(IsInnerArchive ? ExtractedPath : Path)) {
                RARHandler.PasswordRequired += HandlePasswordProtected;
                RARHandler.ExtractionProgress += HandleProgression;
                try {
                    // TODO: Update destination path.
                    RARHandler.Open(RAR.OpenMode.Extract);
                    var flags = (RAR.ArchiveFlags) RARHandler.arcData.Flags;
                    var isFirstVolume = flags.HasFlag(RAR.ArchiveFlags.FirstVolume);
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);

                    if (isVolume && !isFirstVolume) {
                        MessageBox.Show("Archive is not the first volume. Archive will not be processed.", "Cannot process second volume", MessageBoxButtons.OK);
                        throw new IOException("Archive wasn't first volumne.");
                    }
                    
                    while (RARHandler.ReadHeader()) {
                        if (ExtractFile(RARHandler)) {
                            // TODO: Something
                        } else
                        {
                            RARHandler.Skip();
                        }
                    }

                    RARHandler.Close();
                } catch (Exception e) {
                    DPCommon.WriteToLog($"An unexpected error occured while processing for RAR Archive. REASON: {e}");
                }
            }
            ProgressCombo?.Remove();
        }
        internal override void Peek()
        {
            mode = Mode.Peek;
            using (var RARHandler = new RAR(IsInnerArchive ? ExtractedPath : Path)) {
                RARHandler.PasswordRequired += HandlePasswordProtected;
                RARHandler.MissingVolume += HandleMissingVolume;
                RARHandler.ExtractionProgress += HandleProgression;
                RARHandler.NewFile += HandleNewFile;

                try {
                    RARHandler.DestinationPath = IOPath.Combine(DPProcessor.TempLocation, IOPath.GetFileNameWithoutExtension(Path));
                    
                    // Create path and see if it exists.
                    Directory.CreateDirectory(RARHandler.DestinationPath);
                    
                    RARHandler.Open(RAR.OpenMode.List);
                    var flags = (RAR.ArchiveFlags) RARHandler.arcData.Flags;
                    var isFirstVolume = flags.HasFlag(RAR.ArchiveFlags.FirstVolume);
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);

                    if (isVolume && !isFirstVolume) {
                        MessageBox.Show("Archive is not the first volume. Archive will not be processed.", "Cannot process second volume", MessageBoxButtons.OK);
                        throw new IOException("Archive wasn't first volumne.");
                    }
                    
                    while (RARHandler.ReadHeader()) {
                        if (TestFile(RARHandler)) {
                            // TODO: Something.
                        }
                    }

                    RARHandler.Close();
                } catch (Exception e) {
                    errored = true;
                    DPCommon.WriteToLog($"An unexpected error occured while processing for RAR Archive. REASON: {e}");
                }
            }
        }

        internal override void ReleaseArchiveHandles() { }
        #endregion

        private bool ExtractFile(RAR handler) {
            string fileName = handler.CurrentFile.FileName;
            bool changedAttributes = false;
            EXTRACT:
            DPAbstractFile file = null;
            try {
                if (DPFile.FindFileInDPFiles(fileName, out DPFile file1)) file = file1;
                if (file == null) file = Contents.Find(a => a.Path == fileName);
                if (file != null && file.WillExtract)
                {
                    handler.DestinationPath = IOPath.GetDirectoryName(file.TargetPath);
                    // Create folders for the destination path if needed.
                    try
                    {
                        Directory.CreateDirectory(handler.DestinationPath);
                    }
                    catch { }
                    handler.Extract(file.TargetPath);

                    // Only update if we didn't error.
                    file.ExtractedPath = file.TargetPath;
                    file.WasExtracted = true;
                }
                else return false;
            } catch (IOException e) {
                // We errored :(.
                if (file != null) file.errored = true;
                if (e.Message == "File CRC Error" || e.Message == "File could not be opened.") {
                    var flags = (RAR.ArchiveFlags) handler.arcData.Flags;
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);
                    var continuesNext = handler.CurrentFile.ContinuedOnNext;
                    var isEncrypted = handler.CurrentFile.encrypted;

                    if ((!isVolume || !continuesNext) && !isEncrypted)
                    {
                        DPCommon.WriteToLog("File CRC error.");
                    } else {
                        DPCommon.WriteToLog("An unexpected error occured when extracting a rar file.");
                    }
                    
                }
                // Check to see if we are attempting to overwrite a file that we don't have access to (ex: hidden/read-only/anti-virus/user no access).
                if (e.Message == "File write error." || e.Message == "File read error." || e.Message == "File could not be opened.")
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file.TargetPath);
                        if (fileInfo.Exists && !changedAttributes)
                        {
                            fileInfo.Attributes = FileAttributes.Normal;
                            changedAttributes = true;
                            goto EXTRACT;
                        }
                        else
                            DPCommon.WriteToLog($"Failed to extract file even after file attribute change for {fileName}.");
                    } catch (Exception ex)
                    {
                        DPCommon.WriteToLog($"Unable to extract file and change file attributes for {fileName}. REASON: {ex}");
                    }
                }

                return false;
            }
            return true;
        }

        private bool TestFile(RAR handler) {
            try {
                // I'm not sure if UnpackedSize returns negative if the file is partial.
                TrueArchiveSize += (ulong) Math.Max((long) 0, handler.CurrentFile.UnpackedSize);
                handler.Test();
            } catch (IOException e) {
                if (e.Message == "File CRC Error" || e.Message == "File could not be opened.") {
                    var flags = (RAR.ArchiveFlags) handler.arcData.Flags;
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);
                    var continuesNext = handler.CurrentFile.ContinuedOnNext;
                    var isEncrypted = handler.CurrentFile.encrypted;
                    
                    if ((!isVolume || !continuesNext) && !isEncrypted)
                    {
                        return false;
                        // TODO: Call error tab to handle this matter.
                        throw new FileFormatException("File CRC error.");
                    } else {
                        return false;
                        // TODO: Call error tab to handle this matter.
                        throw new FileFormatException("Another error occurred.");
                    }
                }
            }
            return true;
        }
    }
}
