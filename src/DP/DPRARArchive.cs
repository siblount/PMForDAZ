// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using DAZ_Installer.External;
using IOPath = System.IO.Path;
using System.IO;
using DAZ_Installer.DP;

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
        public Control[] progressCombo;
        private List<string> lastVolumes { get; } = new List<string>();
        private Dictionary<string, string> volumePairs = new Dictionary<string, string>(); // First key is the OLD nonworking one, Second key is the working one.

        internal override bool CanPeek { get => true; }
        protected char[] password;

        protected char internalDictSeperator = '\\';
        /// <summary>
        /// Parent of folder. Handles child parent relations.
        /// </summary>
        private bool countingFiles = false;


        public DPRARArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase)
        {

        }

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
                    new DPFolder(e.fileInfo.FileName, null);
                    //newDir.parent = workingArchive.FindParent(ref IDP);
                    //if (newDir.parent == null) workingArchive.rootFolders.Add(newDir);
                }

            }
            else
            {
                if (DPFile.ValidImportExtension(IOPath.GetExtension(e.fileInfo.FileName)))
                {
                    // File is archive.
                    var newArchive = CreateNewArchive(e.fileInfo.FileName, true, RelativePath);
                    newArchive.ParentArchive = DPProcessor.workingArchive;
                }
                else
                {
                    var newFile = new DPFile(e.fileInfo.FileName, null);
                    newFile.AssociatedArchive = DPProcessor.workingArchive;
                }
            }
            if (countingFiles) DPProcessor.workingArchiveFileCount++;
        }


        public void ConnectVolumeDir(string dirPath)
        {
            lastVolumes.Add(System.IO.Path.GetFileName(dirPath));
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

        

        
        // Delete?
        internal void FinalizeFolderStructure()
        {
            RootFolders.Clear();
            foreach (var folder in Folders.Values)
            {
                folder.Parent = null;
                folder.subfolders.Clear();
            }
            foreach (var folder in Folders.Values)
            {
                //folder.parent = null;
                // Find all folders that contain path.
                var childFolders = DPFolder.FindChildFolders(folder.Path, folder);

                // Now appropriately add child folders.
                foreach (var child in childFolders)
                {
                    child.Parent = folder;
                    folder.subfolders.Add(child);
                }

                // Now find parent for this folder.
                var idp = (DPAbstractFile)folder;
                folder.Parent = FindParent( idp);
            }
            DPCommon.WriteToLog(RootFolders);
        }


        internal override void Extract() {
            using (var RARHandler = new RAR(IsInnerArchive ? ExtractedPath : Path)) {
                RARHandler.NewFile += HandleNewFile;
                RARHandler.PasswordRequired += HandlePasswordProtected;
                RARHandler.MissingVolume += HandleMissingVolume;
                RARHandler.NewVolume += HandleNewVolume;
                RARHandler.ExtractionProgress += HandleProgression;
                try {
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
                            // TODO: Something.
                        }
                    }

                    RARHandler.Close();
                } catch (Exception e) {
                    DPCommon.WriteToLog($"An unexpected error occured while processing for RAR Archive. REASON: {e}");
                }
            }
            // extractPage.AddToList(workingArchive);

            // // Add files to hierachy.
            // extractPage.AddToHierachy(workingArchive);

            // TO DO: Highlight files in red for files that failed to extract.
            // Do this in extractPage.
        }
        internal override void Peek()
        {
            using (var RARHandler = new RAR(IsInnerArchive ? ExtractedPath : Path)) {
                RARHandler.PasswordRequired += HandlePasswordProtected;
                RARHandler.MissingVolume += HandleMissingVolume;
                RARHandler.ExtractionProgress += HandleProgression;

                try {
                    RARHandler.DestinationPath = TEMP_LOCATION + IOPath.GetFileNameWithoutExtension(Path);
                    
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
                    DPCommon.WriteToLog($"An unexpected error occured while processing for RAR Archive. REASON: {e}");
                }
            }
            extractControl.extractPage.AddToList(this);

            // Add files to hierachy.
            extractControl.extractPage.AddToHierachy(this);

            // TO DO: Highlight files in red for files that failed to extract.
            // Do this in extractPage.

        }

        private bool ExtractFile(RAR handler) {
            try {
                handler.Extract();
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

        private bool TestFile(RAR handler) {
            try {
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
