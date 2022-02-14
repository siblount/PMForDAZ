// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace DAZ_Installer
{
    // GOAL: Extract files through RAR. While it discovers files, add it to list.
    // Then, deeply analyze each file; determine best approach; and execute best approach (or ask).
    // Lastly, clean up.
    internal static class DPProcessor
    {
        // SecureString - System.Security
        public static string TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
        public static string destinationPath = @"D:\dazinstallertest\";
        public static DPArchive workingArchive;
        public static List<string> doNotProcessList { get; } = new List<string>();
        public static bool countingFiles = true;
        public static uint workingArchiveFileCount { get; set; } =  0; // can disgard.
        public static string GetDestinationPath(string relativePath)
        {
            return Path.Combine(destinationPath, relativePath);
        }

        private static void HandleSupplementary(ref DPArchive archiveFile)
        {
            DPFile supplement = archiveFile.supplementFile;
            var manifestParser = new DSXParser(supplement.extractedPath);
            var parsedFile = manifestParser.GetDSXFile();
            var elements = parsedFile.GetAllElements();
            foreach (var element in elements)
            {
                
                if (new string(element.tagName) == "ProductName")
                {
                    archiveFile.productName = element.attributes["VALUE"];
                    return;
                }
            }
        }

        public static DPArchive ProcessArchive(ref DPArchive archiveFile)
        {
            if (!DPFile.initalized) DPFile.Initalize();

            TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
            try
            {
                Directory.CreateDirectory(TEMP_LOCATION);
            }
            catch (Exception e){ DPCommon.WriteToLog($"Unable to crate directory. {e}"); }
            archiveFile.Extract();
            // Determine content folders.
            foreach (var folder in archiveFile.folders.Values)
            {
                folder.isContentFolder = folder.DetermineIfContentFolder();
            }
            // Scan for manifest and supplement.dsx
            archiveFile.manifestFile = archiveFile.FindFileViaName("Manifest.dsx") as DPFile; // null if not found
            archiveFile.supplementFile = archiveFile.FindFileViaName("Supplement.dsx") as DPFile; // null if not found

            archiveFile.UpdateFilePaths();

            HandleMoveOperations(ref archiveFile);
            if (archiveFile.supplementFile != null) HandleSupplementary(ref archiveFile);

            DPCommon.WriteToLog("We are done");

            // Release some memory.
            if (archiveFile.progressCombo != null)
            {
                extractControl.extractPage.DeleteProgressionCombo(archiveFile.progressCombo);
                archiveFile.progressCombo = null;
            }

            GC.Collect();
            

            for (var i = 0; i < archiveFile.internalArchives.Count; i++)
            {
                var arc = archiveFile.internalArchives[i];
                ProcessArchive(ref arc);
            }


            archiveFile.type = archiveFile.DetermineArchiveType();
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
            DPCommon.WriteToLog($"Archive Type: {archiveFile.type}");
            if (archiveFile.type == ArchiveType.Product)
            {
                
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }

            return archiveFile;
        }
        
        public static DPArchive ProcessArchive(string filePath)
        {
            if (!DPFile.initalized) DPFile.Initalize();
            TEMP_LOCATION = Path.Combine(DPSettings.tempPath, @"DazProductInstaller\");
            try
            {
                Directory.CreateDirectory(TEMP_LOCATION);
            }
            catch (Exception e) { DPCommon.WriteToLog($"Unable to crate directory. {e}"); }

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
            var archiveFile = new DPArchive(filePath, TEMP_LOCATION);
            archiveFile.Extract();

            // Determine content folders.
            foreach (var folder in archiveFile.folders.Values)
            {
                folder.isContentFolder = folder.DetermineIfContentFolder();
            }
            // Scan for manifest and supplement.dsx
            archiveFile.manifestFile = archiveFile.FindFileViaName("Manifest.dsx") as DPFile;
            archiveFile.supplementFile = archiveFile.FindFileViaName("Supplement.dsx") as DPFile;

            archiveFile.UpdateFilePaths();
            var marqueeProgressBar = ShowMarqueeCombo();
            marqueeProgressBar[1].Text = "Moving files...";
            HandleMoveOperations(ref archiveFile);
            if (archiveFile.supplementFile != null) HandleSupplementary(ref archiveFile);

            DPCommon.WriteToLog("We are done");

            // Release some memory.
            if (archiveFile.progressCombo != null)
            {
                extractControl.extractPage.DeleteProgressionCombo(archiveFile.progressCombo);
                archiveFile.progressCombo = null;
            }

            GC.Collect();
            marqueeProgressBar[1].Text = "Analyzing file contents...";
            extractControl.extractPage.mainProcLbl.Text = marqueeProgressBar[1].Text;
            archiveFile.type = archiveFile.DetermineArchiveType();
            DPCommon.WriteToLog("Analyzing files...");
            archiveFile.QuickAnalyzeFiles();
            marqueeProgressBar[1].Text = "Creating library item...";
            extractControl.extractPage.mainProcLbl.Text = marqueeProgressBar[1].Text;
            archiveFile.GetTags();
            RemoveAnalysisProgressCombo(marqueeProgressBar);

            for (var i = 0; i < archiveFile.internalArchives.Count; i++)
            {
                var arc = archiveFile.internalArchives[i];
                ProcessArchive(ref arc);
            }

            DPCommon.WriteToLog($"Archive Type: {archiveFile.type}");
            // Create records and save it to disk.
            var record = archiveFile.CreateRecords();
            if (archiveFile.type == ArchiveType.Product)
            {
                // Add it to the library.
                Library.self.AddNewLibraryItem(record);
            }
            if (!archiveFile.isInnerArchive)
            {
                Library.self.GenerateLibraryItemsFromDisk();
            }
            return archiveFile;
        }

        public static void ProcessRAR(ref DPArchive archive)
        {
            workingArchive = archive;
            countingFiles = true;
            // Get referenced extractPage to get extractControl instance.
            extractControl extractPage = extractControl.extractPage;
            RAR RARHandler = null;
            try
            {
                if (archive.isInnerArchive) RARHandler = new RAR(archive.extractedPath);
                else RARHandler = new RAR(archive.path);
                // Add event listeners.
                RARHandler.NewVolume += new RAR.NewVolumeHandler(HandleNewVolume);
                RARHandler.MissingVolume += new RAR.MissingVolumeHandler(extractPage.HandleMissingVolume);
                RARHandler.PasswordRequired += new RAR.PasswordRequiredHandler(extractPage.HandlePasswordProtected);
                RARHandler.NewFile += new RAR.NewFileHandler(HandleNewFile);

                RARHandler.Open(RAR.OpenMode.List);

                // STOP and prompt if volume & volume is not the first volume
                if ((RARHandler.arcData.Flags & 0x0100) == 0 && (RARHandler.arcData.Flags & 0x0001) != 0)
                {
                    extractPage.DoPromptMessage("Archive is not the first volume. Archive will not be processed.", "Cannot process second volume", MessageBoxButtons.OK);
                    throw new IOException("Archive wasn't first volume.");
                }
                // Use this function to trigger NewFileHandler.
                while (RARHandler.ReadHeader())
                {
                    try
                    {
                        RARHandler.Test();
                    }
                    catch (IOException e)
                    {
                        if (e.Message == "File CRC Error" || e.Message == "File could not be opened.")
                        {
                            // Check archive to see if the archive is a volume archive.
                            var archiveIsVolume = (RARHandler.arcData.Flags & 0x01) == 1;
                            // Check file data to see if it continues on next volume.
                            var fileContinuesNext = RARHandler.CurrentFile.ContinuedOnNext;
                            var fileEncrypted = RARHandler.CurrentFile.encrypted;
                            if ((!archiveIsVolume || !fileContinuesNext) && !fileEncrypted)
                            {
                                // TODO: Call error tab to handle this matter.
                                throw new FileFormatException("File CRC error.");
                            }
                        }
                        else
                        {
                            // TODO: Call error tab to handle this matter.
                            throw new FileFormatException("Another error occurred.");
                        }
                    }
                }

                countingFiles = false;
                // Close file.
                RARHandler.Close();
                RARHandler.Dispose();

                // Reopen and extract.
                if (archive.isInnerArchive) RARHandler = new RAR(archive.extractedPath);
                else RARHandler = new RAR(archive.path);

                // Destination Path
                RARHandler.DestinationPath = TEMP_LOCATION + Path.GetFileNameWithoutExtension(archive.path);
                // Create path and see if it exists.
                Directory.CreateDirectory(RARHandler.DestinationPath);

                RARHandler.Open(RAR.OpenMode.Extract);
                // Create events.
                RARHandler.MissingVolume += new RAR.MissingVolumeHandler(RARHandleVolumes);
                RARHandler.PasswordRequired += new RAR.PasswordRequiredHandler(RARHandlePassword);
                RARHandler.ExtractionProgress += new RAR.ExtractionProgressHandler(extractPage.HandleProgressionRAR);
                while (RARHandler.ReadHeader())
                {
                    // TO DO : Get DPFile and extract to destination and see if it's being extracted.
                    try
                    {
                        RARHandler.Extract();
                    } catch (IOException e)
                    {
                        if (e.Message == "File CRC Error" || e.Message == "File could not be opened.")
                        {
                            // Check archive to see if the archive is a volume archive.
                            var archiveIsVolume = (RARHandler.arcData.Flags & 0x01) == 1;
                            // Check file data to see if it continues on next volume.
                            var fileContinuesNext = RARHandler.CurrentFile.ContinuedOnNext;
                            var fileEncrypted = RARHandler.CurrentFile.encrypted;
                            if ((!archiveIsVolume || !fileContinuesNext) && !fileEncrypted)
                            {
                                // TODO: Call error tab to handle this matter.
                                throw new FileFormatException("File CRC error.");
                            }
                        }
                        else
                        {
                            // TODO: Call error tab to handle this matter.
                            DPCommon.WriteToLog(e);
                            throw new FileFormatException("Another error occurred.");
                        }
                    }
                }

            } catch (Exception e) {
                // TODO: Call error tab to handle this matter. (Probably issue with Archive)
                DPCommon.WriteToLog(e);
            } finally
            {
                try
                {
                    RARHandler.Close();
                    RARHandler.Dispose();
                }
                catch { }
            }
            //workingArchive.FinalizeFolderStructure();
            extractPage.AddToList(ref workingArchive);

            // Add files to hierachy.
            extractPage.AddToHierachy(ref workingArchive);


            // TO DO: Highlight files in red for files that failed to extract.
            // Do this in extractPage.

        }

        public static void ProcessZIP(ref DPArchive archive)
        {
            workingArchive = archive;

            ZipArchive zipArchive;
            if (archive.isInnerArchive) zipArchive = ZipFile.OpenRead(archive.extractedPath);
            else zipArchive = ZipFile.OpenRead(archive.path);

            // Serialize contents.
            SerializeZIPFiles(zipArchive.Entries);

            // Add files to list & hierachy.
            extractControl.extractPage.AddToList(ref archive);
            extractControl.extractPage.AddToHierachy(ref archive);
            //archive.FinalizeFolderStructure();
            // Extract files to temp location.
            var tempLocation = TEMP_LOCATION + Path.GetFileNameWithoutExtension(archive.path);
           
            SafeExtractFiles(ref zipArchive, tempLocation);

            // TO DO: Remove any files that wasn't extracted.
            // Exclude archives.

        }

        public static void Process7Z(ref DPArchive archive)
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
            if (archive.isInnerArchive)
            process.StartInfo.ArgumentList.Add(archive.extractedPath);
            else
            process.StartInfo.ArgumentList.Add(archive.path);

            Serialize7ZContents(ref process);


        }

        // Called before actually extracted.
        #region RAR Handling
        public static void HandleNewFile(RAR sender, NewFileEventArgs e)
        {
            DPCommon.WriteToLog(e.fileInfo.FileName);
            if (e.fileInfo.IsDirectory)
            {
                if (!workingArchive.FolderExists(e.fileInfo.FileName))
                {
                    new DPFolder(e.fileInfo.FileName, null);
                    //newDir.parent = workingArchive.FindParent(ref IDP);
                    //if (newDir.parent == null) workingArchive.rootFolders.Add(newDir);
                }

            } else
            {
                if (DPFile.ValidImportExtension(Path.GetExtension(e.fileInfo.FileName)))
                {
                    // File is archive.
                    var newArchive = new DPArchive(e.fileInfo.FileName,innerArchive: true);
                    newArchive.rootArchive = workingArchive;
                } else
                {
                    var newFile = new DPFile(e.fileInfo.FileName, null);
                    newFile.associatedArchive = workingArchive;
                }
            }
            if (countingFiles) workingArchiveFileCount++; 
        }

        public static void HandleNewVolume(RAR sender, NewVolumeEventArgs e)
        {
            if (sender.ArchivePathName != e.VolumeName) workingArchive.ConnectVolumeDir(e.VolumeName);
            if (DPExtractJob.workingJob.doNotProcess.Contains(Path.GetFileName(sender.ArchivePathName))) {
                DPExtractJob.workingJob.doNotProcess.Add(Path.GetFileName(sender.ArchivePathName));
            }
        }

        public static void RARHandlePassword(RAR sender, PasswordRequiredEventArgs e)
        {
            var password = workingArchive.GetPassword();
            if (string.IsNullOrEmpty(password))
            {
                extractControl.extractPage.HandlePasswordProtected(sender, e);
                return;
            }
            e.Password = workingArchive.GetPassword();
            e.ContinueOperation = !workingArchive.cancelledOperation;
            workingArchive.secondPasswordPromptHasSeen = true;
        }

        public static void RARHandleVolumes(RAR _, MissingVolumeEventArgs e)
        {
            e.VolumeName = workingArchive.GetRightVolume(e.VolumeName);
        }
        #endregion
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
                // If entry is a valid archive. Treat it as a DPArchive.
                else if (DPFile.ValidImportExtension(Path.GetExtension(entry.Name))) {
                    var newArchive = new DPArchive(entry.FullName, innerArchive: true);
                    newArchive.rootArchive = workingArchive;

                    workingArchive.contents.Add(newArchive);
                }
                else
                {
                    var newFile = new DPFile(entry.FullName, null);
                    newFile.associatedArchive = workingArchive;
                }
            }
            workingArchive.fileCount = (uint) entries.Count;
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
                dpFile.extractedPath = cleanedName;
                else
                {
                    success = DPArchive.FindArchiveViaName(file.FullName, out DPArchive dpArchive);
                    if (success) dpArchive.extractedPath = cleanedName;
                }
                Directory.CreateDirectory(cleanedDest);
                try
                {
                    file.ExtractToFile(cleanedName, true);
                } catch (Exception e)
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
        internal static bool GetMultiParts(ref DPArchive archive, out string[] otherArchiveNames)
        {
            // Since the inital call did not throw an error, we can assume that there are valid multipart names.
            var similarFiles = Directory.GetFiles(Path.GetDirectoryName(archive.extractedPath), Path.GetFileNameWithoutExtension(archive.extractedPath));
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
                if (numList[i] - numList[i-1] != -1)
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
                } else
                {
                    if (GetContents7z(msg, out string[] files))
                    {
                        foreach (var file in files)
                        {
                            if (DPFile.ValidImportExtension(Path.GetExtension(file))) {
                                var newArchive = new DPArchive(file, innerArchive: true);
                                newArchive.rootArchive = workingArchive;

                                workingArchive.contents.Add(newArchive);
                            } else
                            {
                                var newFile = new DPFile(file,null);

                                newFile.associatedArchive = workingArchive;
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
        public static Dictionary<string, string> HandleManifest(ref DPFile manifest, ref DPArchive archive)
        {
            var manifestParser = new DSXParser(manifest.extractedPath);
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
                        } else if (target == "Application")
                        {

                        }
                    }
                }
            } catch { }
            return workingDict;
        }

        public static void HandleMoveOperations(ref DPArchive archive)
        {
            // Handle Manifest first.
            if (archive.manifestFile != null)
            {
                if (DPSettings.handleInstallation == InstallOptions.ManifestAndAuto ||
                    DPSettings.handleInstallation == InstallOptions.ManifestOnly)
                {
                    var manifest = archive.manifestFile;
                    Dictionary<string, string> manifestDestinations = HandleManifest(ref manifest, ref archive);

                    foreach (var file in archive.contents)
                    {
                        if (manifestDestinations.ContainsKey(file.path))
                        {
                            bool wasSuccessful = true;
                            try
                            {
                                file.destinationPath = manifestDestinations[archive.path];
                                // TO DO: Add directories if does not exist.
                                Directory.CreateDirectory(Path.GetDirectoryName(file.destinationPath));
                                File.Move(file.extractedPath, file.destinationPath, true);
                            }
                            catch
                            {
                                wasSuccessful = false;
                            }

                            if (file.GetType() == typeof(DPFile))
                            {
                                ((DPFile)file).errored = wasSuccessful;
                            }
                            else if (file.GetType() == typeof(DPArchive))
                            {
                                ((DPArchive)file).errored = wasSuccessful;
                            }
                            file.wasExtracted = wasSuccessful;
                        }
                        else
                        {
                            if (DPSettings.handleInstallation == InstallOptions.ManifestOnly)
                            {
                                file.extract = false;
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
                var folders = archive.folders.Values.ToArray();
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
                            var dPath = Path.Combine(DPSettings.destinationPath, child.relativePath);
                            // Update child destination path.
                            child.destinationPath = dPath;
                            var successfulMove = true;
                            try
                            {
                                child.extract = true;
                                // Move.
                                Directory.CreateDirectory(Path.GetDirectoryName(child.destinationPath));

                                File.Move(child.extractedPath, child.destinationPath);
                            }
                            catch (Exception e)
                            {
                                successfulMove = false;
                                DPCommon.WriteToLog($"Unable to move file. REASON: {e}");
                            }
                            child.wasExtracted = successfulMove;
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
                        if (file.GetType() == typeof(DPArchive))
                        {
                            var arc = (DPArchive)file;
                            // Add to queue.
                            workingArchive.internalArchives.Add(arc);
                        }
                    }
                }

                // Hunt down all files in root content.

                foreach (var content in archive.rootContents)
                {
                    if (content.GetType() == typeof(DPArchive))
                    {
                        var arc = (DPArchive) content;
                        // Add to queue.
                        workingArchive.internalArchives.Add(arc);
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
            var progressCombo = (Control[]) extractControl.extractPage.Invoke(new Func<Control[]>(extractControl.extractPage.createProgressComboMarquee));
            var progressLabel = (Label) progressCombo[1];
            var progressBar = (ProgressBar) progressCombo[2];

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
