// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using DAZ_Installer.External;

namespace DAZ_Installer.DP
{
    // GOAL: Extract files through RAR. While it discovers files, add it to list.
    // Then, deeply analyze each file; determine best approach; and execute best approach (or ask).
    // Lastly, clean up.
    internal static class DPProcessor
    {
        // SecureString - System.Security
        public static string TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
        public static string destinationPath = @"D:\dazinstallertest\";
        public static DPAbstractArchive workingArchive;
        public static HashSet<string> previouslyInstalledArchiveNames { get; } = new HashSet<string>();
        public static List<string> doNotProcessList { get; } = new List<string>();
        public static uint workingArchiveFileCount { get; set; } = 0; // can disgard.
        public static string GetDestinationPath(string relativePath)
        {
            return Path.Combine(destinationPath, relativePath);
        }

        private static void HandleSupplementary(ref DPAbstractArchive archiveFile)
        {
            DPFile supplement = archiveFile.SupplementFile;
            var manifestParser = new DSXParser(supplement.ExtractedPath);
            var parsedFile = manifestParser.GetDSXFile();
            var elements = parsedFile.GetAllElements();
            foreach (var element in elements)
            {

                if (new string(element.tagName) == "ProductName")
                {
                    archiveFile.ProductInfo.ProductName = element.attributes["VALUE"];
                    return;
                }
            }
        }

        public static DPAbstractArchive ProcessArchive(ref DPAbstractArchive archiveFile)
        {
            TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
            try
            {
                Directory.CreateDirectory(TEMP_LOCATION);
            }
            catch (Exception e) { DPCommon.WriteToLog($"Unable to crate directory. {e}"); }
            archiveFile.Extract();
            // Determine content folders.
            foreach (var folder in archiveFile.Folders.Values)
            {
                folder.isContentFolder = folder.DetermineIfContentFolder();
            }
            // Scan for manifest and supplement.dsx
            archiveFile.ManifestFile = archiveFile.FindFileViaName("Manifest.dsx") as DPFile; // null if not found
            archiveFile.SupplementFile = archiveFile.FindFileViaName("Supplement.dsx") as DPFile; // null if not found

            archiveFile.UpdateFilePaths();

            HandleMoveOperations(ref archiveFile);
            if (archiveFile.SupplementFile != null) HandleSupplementary(ref archiveFile);

            DPCommon.WriteToLog("We are done");

            // Release some memory.

            archiveFile.ProgressCombo?.Remove();

            for (var i = 0; i < archiveFile.InternalArchives.Count; i++)
            {
                var arc = archiveFile.InternalArchives[i];
                ProcessArchive(ref arc);
            }


            archiveFile.Type = archiveFile.DetermineArchiveType();
            DPCommon.WriteToLog("Analyzing files...");
            // Parse files.
            archiveFile.QuickAnalyzeFiles();
            try
            {
                archiveFile.GetTags();
            }
            catch (Exception e)
            {

                DPCommon.WriteToLog($"Failed to get tags: {e}");
            }

            // Create record.
            var record = archiveFile.CreateRecords();
            // TO DO: Only add if successful extraction, and all files from temp were moved, and/or user didn't cancel operation.
            DPCommon.WriteToLog($"Archive Type: {archiveFile.Type}");
            if (archiveFile.Type == ArchiveType.Product)
            {
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }

            return archiveFile;
        }

        public static DPAbstractArchive ProcessArchive(string filePath)
        {

            // Update our temp location in case user changed the settings.
            TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
            try
            {
                Directory.CreateDirectory(TEMP_LOCATION);
            }
            catch (Exception e) { DPCommon.WriteToLog($"Unable to create directory. {e}"); }

            if (LibraryIO.previouslyInstalledArchives.Contains(Path.GetFileName(filePath)))
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
            // Create new DPFile.
            var archiveFile = DPAbstractArchive.CreateNewArchive(filePath, false, TEMP_LOCATION);
            archiveFile.Extract();

            // Determine content folders.
            foreach (var folder in archiveFile.Folders.Values)
            {
                folder.isContentFolder = folder.DetermineIfContentFolder();
            }
            // Scan for manifest and supplement.dsx
            archiveFile.ManifestFile = archiveFile.FindFileViaName("Manifest.dsx") as DPFile;
            archiveFile.SupplementFile = archiveFile.FindFileViaName("Supplement.dsx") as DPFile;

            archiveFile.UpdateFilePaths();

            // TODO: Ensure that archive progress combo is not null.
            archiveFile.ProgressCombo.ChangeProgressBarStyle(true);
            archiveFile.ProgressCombo.ProgressBarLbl.Text = "Moving files...";
            HandleMoveOperations(ref archiveFile);
            if (archiveFile.SupplementFile != null) HandleSupplementary(ref archiveFile);

            DPCommon.WriteToLog("We are done");

            // Release some memory.
            archiveFile.ProgressCombo?.Remove();

            marqueeProgressBar[1].Text = "Analyzing file contents...";
            extractControl.extractPage.mainProcLbl.Text = marqueeProgressBar[1].Text;
            archiveFile.Type = archiveFile.DetermineArchiveType();
            DPCommon.WriteToLog("Analyzing files...");
            archiveFile.QuickAnalyzeFiles();
            marqueeProgressBar[1].Text = "Creating library item...";
            extractControl.extractPage.mainProcLbl.Text = marqueeProgressBar[1].Text;
            archiveFile.GetTags();
            RemoveAnalysisProgressCombo(marqueeProgressBar);

            for (var i = 0; i < archiveFile.InternalArchives.Count; i++)
            {
                var arc = archiveFile.InternalArchives[i];
                ProcessArchive(ref arc);
            }

            DPCommon.WriteToLog($"Archive Type: {archiveFile.Type}");
            // Create records and save it to disk.
            var record = archiveFile.CreateRecords();
            if (archiveFile.Type == ArchiveType.Product)
            {
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }
            if (!archiveFile.IsInnerArchive)
            {
                //Library.self.GenerateLibraryItemsFromDisk();
                Library.self.InformLibraryUpdate();
            }
            return archiveFile;
        }

        public static void ProcessZIP(ref DPAbstractArchive archive)
        {
            workingArchive = archive;

            ZipArchive zipArchive;
            if (archive.IsInnerArchive) zipArchive = ZipFile.OpenRead(archive.ExtractedPath);
            else zipArchive = ZipFile.OpenRead(archive.Path);

            // Serialize contents.
            SerializeZIPFiles(zipArchive.Entries);

            // Add files to list & hierachy.
            extractControl.extractPage.AddToList(archive);
            extractControl.extractPage.AddToHierachy(archive);
            //archive.FinalizeFolderStructure();
            // Extract files to temp location.
            var tempLocation = TEMP_LOCATION + Path.GetFileNameWithoutExtension(archive.Path);

            SafeExtractFiles(ref zipArchive, tempLocation);

            // TO DO: Remove any files that wasn't extracted.
            // Exclude archives.

        }

        public static void Process7Z(ref DPAbstractArchive archive)
        {
            // Call our 7za.exe app.
            // Should be 7za.exe
            Process process = new Process();
            process.StartInfo.FileName = "7za.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.ArgumentList.Add("l");
            process.StartInfo.ArgumentList.Add("-slt");
            if (archive.IsInnerArchive)
                process.StartInfo.ArgumentList.Add(archive.ExtractedPath);
            else
                process.StartInfo.ArgumentList.Add(archive.Path);

            Serialize7ZContents(ref process);


        }

        // Called before actually extracted.
        #region ZIP Handling

        public static void SerializeZIPFiles(IReadOnlyCollection<ZipArchiveEntry> entries)
        {

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    // Then it is a folder.
                    if (!workingArchive.FolderExists(entry.FullName))
                    {
                        new DPFolder(entry.FullName, null);
                    }
                }
                // If entry is a valid archive. Treat it as a DPAbstractArchive.
                else if (DPFile.ValidImportExtension(Path.GetExtension(entry.Name)))
                {
                    var newArchive = new DPAbstractArchive(entry.FullName, innerArchive: true);
                    newArchive.ParentArchive = workingArchive;

                    workingArchive.Contents.Add(newArchive);
                }
                else
                {
                    var newFile = new DPFile(entry.FullName, null);
                    newFile.AssociatedArchive = workingArchive;
                }
            }
            workingArchive.FileCount = (uint)entries.Count;
        }
        /// <summary>
        /// Handles zip vulnerabilty and notifies the user of this potential issue. Returns a boolean value if errors occurred or user cancelled.
        /// </summary>
        /// <param name="entries">The zip file archive contents.</param>
        /// <param name="directory">The location to extract the files.</param>
        /// <returns>Returns true if no errors occurred and user did not cancel operation. Otherwise, returns false if an error occurred or user cancelled operation.</returns>
        public static bool SafeExtractFiles(ref ZipArchive archive, string directory)
        {
            var stop = false;
            var i = 0;
            var max = archive.Entries.Count;

            foreach (var file in archive.Entries)
            {
                if (stop == true) break;
                // No folders allowed.
                if (string.IsNullOrEmpty(file.Name)) continue;
                var success = DPFile.FindFileInDPFiles(file.FullName, out DPFile dpFile);
                extractControl.extractPage.HandleProgressionZIP(ref archive, i, max);
                var cleanedDest = Path.Combine(directory, Path.GetDirectoryName(file.FullName));
                var cleanedName = Path.Combine(directory, file.FullName);
                if (success)
                    dpFile.ExtractedPath = cleanedName;
                else
                {
                    success = DPAbstractArchive.FindArchiveViaName(file.FullName, out DPAbstractArchive dpArchive);
                    if (success) dpArchive.ExtractedPath = cleanedName;
                }
                Directory.CreateDirectory(cleanedDest);
                try
                {
                    file.ExtractToFile(cleanedName, true);
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to extract {file.Name}. REASON: {e}");
                }
                i++;
            }
            extractControl.extractPage.HandleProgressionZIP(ref archive, max, max);
            return true;
        }
        #endregion
        #region 7Z Handling
        internal static bool GetMultiParts(ref DPAbstractArchive archive, out string[] otherArchiveNames)
        {
            // Since the inital call did not throw an error, we can assume that there are valid multipart names.
            var similarFiles = Directory.GetFiles(Path.GetDirectoryName(archive.ExtractedPath), Path.GetFileNameWithoutExtension(archive.ExtractedPath));
            var numList = new List<int>(similarFiles.Length);
            var possibleArchiveNames = new List<string>(similarFiles.Length);
            foreach (var file in similarFiles)
            {
                var ext = Path.GetExtension(file).Substring(1); // 0001
                if (int.TryParse(ext, out int num))
                {
                    numList.Add(num);
                    possibleArchiveNames.Add(file);
                }
            }

            for (var i = numList.Count - 1; i > 0; i--)
            {
                if (numList[i] - numList[i - 1] != -1)
                {
                    otherArchiveNames = null;
                    return false;
                }
            }

            possibleArchiveNames.Sort();
            otherArchiveNames = possibleArchiveNames.ToArray();
            return true;

        }
        internal static void Serialize7ZContents(ref Process process)
        {
            // Check to see if we got something.
            if (GetMessage7z(ref process, out string msg))
            {
                if (CheckForErrors(msg, out string errorMsg))
                {
                    // Add to error msg list.
                    DPCommon.WriteToLog(errorMsg);
                }
                else
                {
                    if (GetContents7z(msg, out string[] files))
                    {
                        foreach (var file in files)
                        {
                            if (DPFile.ValidImportExtension(Path.GetExtension(file)))
                            {
                                var newArchive = new DPAbstractArchive(file, true);
                                newArchive.ParentArchive = workingArchive;

                                workingArchive.Contents.Add(newArchive);
                            }
                            else
                            {
                                var newFile = new DPFile(file, null);

                                newFile.AssociatedArchive = workingArchive;
                            }
                        }
                    }

                }
            }

        }
        internal static bool CheckForErrors(string msg, out string errorMsg)
        {
            var lines = msg.Split("\r\n");
            var catchingErrors = false;
            var errorMsgs = new List<string>(2);
            foreach (var line in lines)
            {
                if (line == "Errors:") catchingErrors = true;
                if (catchingErrors)
                {
                    if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) break;
                    errorMsgs.Add(line);
                }
            }
            if (errorMsgs.Count == 0) errorMsg = null;
            else errorMsg = string.Join('\n', errorMsgs.ToArray());
            return catchingErrors;
        }

        internal static string[] GetFileInfo7z(string msg)
        {
            var info = msg.Split("\r\n\r\n").ToList();
            info.RemoveAt(info.Count - 1);
            return info.ToArray();
        }

        internal static bool GetContents7z(string msg, out string[] contents)
        {
            const string fileInfoHeader = "----------\r\n";
            // Only spits out files, not folders.
            if (msg.Contains(fileInfoHeader))
            {
                // Then we got work to do.
                var contentBlocks = GetFileInfo7z(msg.Substring(msg.IndexOf(fileInfoHeader) + fileInfoHeader.Length));
                var fileNames = new List<string>(contentBlocks.Length);
                foreach (var content in contentBlocks)
                {
                    fileNames.Add(content.Split("\r\n")[0].Split('=')[1].Trim());
                }
                contents = fileNames.ToArray();
                return true;
            }
            contents = null;
            return false;
        }

        internal static bool GetMessage7z(ref Process process, out string msg)
        {
            process.Start();
            process.WaitForExit();
            msg = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(msg)) return false;
            return true;
        }
        #endregion
        /// <summary>
        ///  Opens the manifest file, and returns a list of files to extract and location.
        /// </summary>
        /// <param name="manifest">A DPFile that is a manifest.</param>
        /// <returns>Returns a dictionary containing files to extract and their destination. Key is the file path in the archive, and value is the destination.</returns>
        public static Dictionary<string, string> HandleManifest(ref DPFile manifest, ref DPAbstractArchive archive)
        {
            var manifestParser = new DSXParser(manifest.ExtractedPath);
            var parsedFile = manifestParser.GetDSXFile();
            var elements = parsedFile.GetAllElements();

            var workingDict = new Dictionary<string, string>(elements.Length);
            // Key : File Path in archive, Value = Destination.
            try
            {
                foreach (var element in elements)
                {
                    if (element.attributes.ContainsKey("ACTION") && new string(element.tagName) == "File")
                    {
                        var target = element.attributes["TARGET"];
                        if (target == "Content")
                        {
                            // Get value.
                            var filePath = element.attributes["VALUE"];
                            var pathWithoutContent = filePath.Remove(0, 7).TrimStart(PathHelper.GetSeperator(filePath));
                            workingDict[filePath] = Path.Combine(TEMP_LOCATION, pathWithoutContent);
                        }
                        else if (target == "Application")
                        {

                        }
                    }
                }
            }
            catch { }
            return workingDict;
        }

        public static void HandleMoveOperations(ref DPAbstractArchive archive)
        {
            // Handle Manifest first.
            if (archive.ManifestFile != null)
            {
                if (DPSettings.handleInstallation == InstallOptions.ManifestAndAuto ||
                    DPSettings.handleInstallation == InstallOptions.ManifestOnly)
                {
                    var manifest = archive.ManifestFile;
                    Dictionary<string, string> manifestDestinations = HandleManifest(ref manifest, ref archive);

                    foreach (var file in archive.Contents)
                    {
                        if (manifestDestinations.ContainsKey(file.Path))
                        {
                            bool wasSuccessful = true;
                            try
                            {
                                file.DestinationPath = manifestDestinations[archive.Path];
                                // TO DO: Add directories if does not exist.
                                Directory.CreateDirectory(Path.GetDirectoryName(file.DestinationPath));
                                File.Move(file.ExtractedPath, file.DestinationPath, true);
                            }
                            catch
                            {
                                wasSuccessful = false;
                            }

                            file.errored = file.WasExtracted = wasSuccessful;
                        }
                        else
                        {
                            if (DPSettings.handleInstallation == InstallOptions.ManifestOnly)
                            {
                                file.WillExtract = false;
                            }
                        }
                    }

                    // We are done with the dictionary. Remove memory.
                    GC.Collect();


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
                            var dPath = Path.Combine(DPSettings.destinationPath, child.RelativePath);
                            // Update child destination path.
                            child.DestinationPath = dPath;
                            var successfulMove = true;
                            try
                            {
                                child.WillExtract = true;
                                // Move.
                                Directory.CreateDirectory(Path.GetDirectoryName(child.DestinationPath));

                                File.Move(child.ExtractedPath, child.DestinationPath);
                            }
                            catch (Exception e)
                            {
                                successfulMove = false;
                                DPCommon.WriteToLog($"Unable to move file. REASON: {e}");
                            }
                            child.WasExtracted = successfulMove;
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
                        if (file.GetType() == typeof(DPAbstractArchive))
                        {
                            var arc = (DPAbstractArchive)file;
                            // Add to queue.
                            workingArchive.InternalArchives.Add(arc);
                        }
                    }
                }

                // Hunt down all files in root content.

                foreach (var content in archive.RootContents)
                {
                    if (content.GetType() == typeof(DPAbstractArchive))
                    {
                        var arc = (DPAbstractArchive)content;
                        // Add to queue.
                        workingArchive.InternalArchives.Add(arc);
                    }
                }
                // Force cleanup.
                GC.Collect();
            }

        }

        // TO DO: Make primary func for progressbar marquee.
        public static Control[] ShowMarqueeCombo()
        {
            DPCommon.WriteToLog("Creating new progression combo, marquee style.");
            var progressStack = extractControl.progressStack;
            var progressCombo = extractControl.extractPage.Invoke(new Func<Control[]>(extractControl.extractPage.createProgressComboMarquee));
            var progressLabel = (Label)progressCombo[1];
            var progressBar = (ProgressBar)progressCombo[2];

            var openSlotIndex = ArrayHelper.GetNextOpenSlot(progressStack);
            if (openSlotIndex == -1)
            {
                throw new IndexOutOfRangeException($"Attempted to add more than {extractControl.progressStack.Length} to array.");
            }
            else
            {
                progressStack[openSlotIndex] = workingArchive;
                extractControl.controlComboStack[openSlotIndex] = progressCombo;
            }

            return progressCombo;
        }

        public static void RemoveAnalysisProgressCombo(Control[] progressCombo)
        {
            if (progressCombo != null)
                extractControl.extractPage.DeleteProgressionCombo(progressCombo);
        }
    }
}
