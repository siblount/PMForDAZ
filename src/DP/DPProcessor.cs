// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace DAZ_Installer.DP
{
    // GOAL: Extract files through RAR. While it discovers files, add it to list.
    // Then, deeply analyze each file; determine best approach; and execute best approach (or ask).
    // Lastly, clean up.
    internal static class DPProcessor
    {
        // SecureString - System.Security
        public static string TempLocation = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
        public static string DestinationPath = DPSettings.destinationPath;
        public static DPAbstractArchive workingArchive;
        public static HashSet<string> previouslyInstalledArchiveNames { get; } = new HashSet<string>();
        public static List<string> doNotProcessList { get; } = new List<string>();
        public static uint workingArchiveFileCount { get; set; } = 0; // can disgard. 
        public static SettingOptions OverwriteFiles = SettingOptions.Yes;

        public static DPAbstractArchive ProcessInnerArchive(DPAbstractArchive archiveFile)
        {
            workingArchive = archiveFile;
            try
            {
                Directory.CreateDirectory(TempLocation);
            }
            catch (Exception e) { DPCommon.WriteToLog($"Unable to create temp directory. {e}"); }
            try
            {
                archiveFile.Peek();
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Unable to peek into inner archive: {Path.GetFileName(archiveFile.Path)}." +
                    $"REASON: {ex}");
            }
            // TO DO: Highlight files in red for files that failed to extract.
            Extract.ExtractPage.AddToList(archiveFile);
            Extract.ExtractPage.AddToHierachy(archiveFile);
            // Check if we have enough room.
            if (!DestinationHasEnoughSpace())
            {
                // TODO: Warn user that there was not enough space and cancel.
                DPCommon.WriteToLog("Destination did not have enough space. Operation aborted.");
                archiveFile.errored = true;
                return archiveFile;
            }

            archiveFile.ManifestFile = archiveFile.FindFileViaNameContains("Manifest.dsx") as DPDSXFile; // null if not found

            PrepareOperations(archiveFile);
            DetermineContentFolders(archiveFile);
            UpdateRelativePaths(archiveFile);

            DetermineFilesToExtract(archiveFile);

            // archiveFile.UpdateFilePaths();
            archiveFile.Extract();

            DPCommon.WriteToLog("We are done");

            archiveFile.ProgressCombo?.Remove();

            var analyzeCombo = new DPProgressCombo();
            analyzeCombo.ChangeProgressBarStyle(true);
            analyzeCombo.UpdateText("Analyzing file contents...");
            archiveFile.Type = archiveFile.DetermineArchiveType();
            DPCommon.WriteToLog("Analyzing files...");
            archiveFile.ReadContentFiles();
            analyzeCombo.UpdateText("Creating library item...");
            try {
                archiveFile.GetTags();
            } catch { DPCommon.WriteToLog("Failed to get tags."); }
            analyzeCombo?.Remove();

            for (var i = 0; i < archiveFile.InternalArchives.Count; i++)
            {
                var arc = archiveFile.InternalArchives[i];
                if (arc.WasExtracted) ProcessInnerArchive(arc);
            }

            // Create record.
            var record = archiveFile.CreateRecords();
            // TO DO: Only add if successful extraction, and all files from temp were moved, and/or user didn't cancel operation.
            DPCommon.WriteToLog($"Archive Type: {archiveFile.Type}");
            // TODO: This might need to be removed.
            if (archiveFile.Type == ArchiveType.Product)
            {
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }


            return archiveFile;
        }

        public static DPAbstractArchive? ProcessArchive(string filePath)
        {
            // We use these variables in case the user changes the settings in mist of an extraction process.
            TempLocation = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
            DestinationPath = DPSettings.destinationPath;
            try
            {
                Directory.CreateDirectory(TempLocation);
            }
            catch (Exception e) { DPCommon.WriteToLog($"Unable to create directory. {e}"); }

            if (previouslyInstalledArchiveNames.Contains(Path.GetFileName(filePath)))
            {
                // ,_, 
                switch (DPSettings.installPrevProducts)
                {
                    case SettingOptions.No:
                        return null;
                    case SettingOptions.Prompt:
                        var result = MessageBox.Show($"It seems that {Path.GetFileName(filePath)} was already processed. Do you wish to continue processing this file?", "Archive already processed", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.No) return null;
                        break;
                }
            }
            // Create new archive.
            var archiveFile = DPAbstractArchive.CreateNewArchive(filePath, false);
            if (archiveFile == null) return null;
            workingArchive = archiveFile;
            try
            {
                archiveFile.Peek();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Unable to peek into inner archive: {Path.GetFileName(archiveFile.Path)}." +
                    $"REASON: {ex}");
            }
            // TO DO: Highlight files in red for files that failed to extract.
            Extract.ExtractPage.AddToList(archiveFile);
            Extract.ExtractPage.AddToHierachy(archiveFile);

            // Check if we have enough room.
            if (!DestinationHasEnoughSpace())
            {
                // TODO: Warn user that there was not enough space and cancel.
                DPCommon.WriteToLog("Destination did not have enough space. Operation aborted.");
                archiveFile.errored = true;
                return archiveFile;
            }

            archiveFile.ManifestFile = archiveFile.FindFileViaNameContains("Manifest.dsx") as DPDSXFile;

            PrepareOperations(archiveFile);
            DetermineContentFolders(archiveFile);
            UpdateRelativePaths(archiveFile);
            // TODO: Ensure that archive progress combo is not null.
            DetermineFilesToExtract(archiveFile);

            archiveFile.Extract();
            DPCommon.WriteToLog("We are done");

            archiveFile.ProgressCombo?.Remove();

            var analyzeCombo = new DPProgressCombo();
            analyzeCombo.ChangeProgressBarStyle(true);
            analyzeCombo.UpdateText("Analyzing file contents...");
            archiveFile.Type = archiveFile.DetermineArchiveType();
            DPCommon.WriteToLog("Analyzing files...");
            archiveFile.ReadContentFiles();
            analyzeCombo.UpdateText("Creating library item...");
            try {
                archiveFile.GetTags();
            } catch { DPCommon.WriteToLog("Failed to get tags."); }
            analyzeCombo?.Remove();
            for (var i = 0; i < archiveFile.InternalArchives.Count; i++)
            {
                var arc = archiveFile.InternalArchives[i];
                if (arc.WasExtracted) ProcessInnerArchive(arc);
            }

            DPCommon.WriteToLog($"Archive Type: {archiveFile.Type}");
            // Create records and save it to disk.
            // TODO: Add a flag to make sure records aren't created for completely
            // failed archives (such as an "zip" archive when really it's a jpg file).
            var record = archiveFile.CreateRecords();
            // TODO: This might need to be removed.
            if (archiveFile.Type == ArchiveType.Product)
            {
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }
            Library.self.InformLibraryUpdate();
            
            return archiveFile;
        }

        private static void UpdateRelativePaths(DPAbstractArchive archive)
        {
            foreach (var content in archive.RootContents)
            {
                content.RelativePath = content.Path;
            }
            foreach (var folder in archive.Folders.Values)
            {
                folder.UpdateChildrenRelativePaths();
            }
            
        }

        public static void DetermineFilesToExtract(DPAbstractArchive archive)
        {
            // Handle Manifest first.
            if (archive.ManifestFile != null && archive.ManifestFile.WasExtracted)
            {
                if (DPSettings.handleInstallation == InstallOptions.ManifestAndAuto ||
                    DPSettings.handleInstallation == InstallOptions.ManifestOnly)
                {
                    var manifest = archive.ManifestFile;
                    var manifestDestinations = manifest.GetManifestDestinations();

                    foreach (var file in archive.Contents)
                    {
                        if (manifestDestinations.ContainsKey(file.Path))
                        {
                            try
                            {
                                file.TargetPath = Path.Combine(DestinationPath, manifestDestinations[file.Path]);
                                file.WillExtract = true;
                                // TO DO: Add directories if does not exist.
                                Directory.CreateDirectory(Path.GetDirectoryName(file.TargetPath));
                            }
                            catch (Exception ex) {
                                DPCommon.WriteToLog($"An error occured while attempting to create directory. REASON: {ex}");
                                file.errored = true;
                            }
                        }
                        else
                        {
                            file.WillExtract = DPSettings.handleInstallation != InstallOptions.ManifestOnly;
                        }
                    }
                }

            }
            if (DPSettings.handleInstallation == InstallOptions.Automatic || DPSettings.handleInstallation == InstallOptions.ManifestAndAuto)
            {
                // Get contents where file was not extracted.
                var folders = archive.Folders.Values.ToArray();
                foreach (var folder in folders)
                {
                    // TO DO: Check if folder is a subfolder of a folder that is a content folder.
                    if (folder.isContentFolder || folder.isPartOfContentFolder)
                    {
                        // Update children's relative path.
                        folder.UpdateChildrenRelativePaths();

                        foreach (var child in folder.GetFiles())
                        {
                            // Get destination path.
                            var dPath = Path.Combine(DestinationPath, child.RelativePath);
                            // Update child destination path.
                            child.TargetPath = dPath;
                            child.WillExtract = true;
                        }
                    }
                }
                // Now hunt down all files in folders that aren't in content folders.
                foreach (var folder in folders)
                {
                    if (folder.isContentFolder) continue;
                    // Add all archives to the inner archives to process for later processing.
                    foreach (var file in folder.GetFiles())
                    {
                        if (file is DPAbstractArchive)
                        {
                            var arc = (DPAbstractArchive)file;
                            arc.WillExtract = true;
                            arc.TargetPath = Path.Combine(TempLocation, arc.RelativePath);
                            // Add to queue.
                            workingArchive.InternalArchives.Add(arc);
                        }
                    }
                }

                // Hunt down all files in root content.

                foreach (var content in archive.RootContents)
                {
                    if (content is DPAbstractArchive)
                    {
                        var arc = (DPAbstractArchive)content;
                        arc.WillExtract = true;
                        arc.TargetPath = Path.Combine(TempLocation, arc.RelativePath);
                        // Add to queue.
                        workingArchive.InternalArchives.Add(arc);
                    }
                }
            }

        }

        private static bool DestinationHasEnoughSpace() {
            var destinationDrive = new DriveInfo(Path.GetPathRoot(DestinationPath));
            return (ulong) destinationDrive.AvailableFreeSpace > workingArchive.TrueArchiveSize;
        }

        private static bool TempHasEnoughSpace() {
            var tempDrive = new DriveInfo(Path.GetPathRoot(TempLocation));
            return (ulong) tempDrive.AvailableFreeSpace > workingArchive.TrueArchiveSize;
        }

        private static void DetermineContentFolders(DPAbstractArchive archiveFile)
        {
            foreach (var folder in archiveFile.Folders.Values)
            {
                folder.isContentFolder = folder.DetermineIfContentFolder();
            }
        }


        // TODO: Clear temp needs to remove as much space as possible. It will error when we have file handles.
        private static void ClearTemp() {
            try {
                if (Directory.Exists(TempLocation)) {
                    Directory.Delete(TempLocation, true);
                }
            } catch {}
        }

        private static void PrepareOperations(DPAbstractArchive archive) {

            if (!archive.CanReadWithoutExtracting) {
                if (!TempHasEnoughSpace()) {
                    ClearTemp();
                    if (!TempHasEnoughSpace()) {
                        DPCommon.WriteToLog("Temp location does not have enough space. Operation aborted.");
                        return;
                    } else {
                        workingArchive.ReadMetaFiles();
                    }
                }
            } else {
                workingArchive.ReadMetaFiles();
            }
        }
    }
}
