using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IOPath = System.IO.Path;

namespace DAZ_Installer.DP {
    internal class DP7zArchive : DPAbstractArchive
    {

        internal DP7zArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase) {
            
        }

        internal override void Extract()
        {
            Process process = new Process();
            process.StartInfo.FileName = "7za.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.ArgumentList.Add("l");
            process.StartInfo.ArgumentList.Add("-slt"); // Show technical information.
            if (IsInnerArchive)
                process.StartInfo.ArgumentList.Add(ExtractedPath);
            else
                process.StartInfo.ArgumentList.Add(Path);
        }

        internal override void Peek()
        {
            var process = Setup7ZProcess();
            // Check to see if we got something.
            if (GetMessage(process, out string msg))
            {
                if (CheckForErrors(msg, out string errorMsg))
                {
                    // Add to error msg list.
                    DPCommon.WriteToLog(errorMsg);
                }
                else
                {
                    if (GetContents(msg, out string[] files))
                    {
                        foreach (var file in files)
                        {
                            var ext = GetExtension(file);
                            if (DPFile.ValidImportExtension(ext))
                            {
                                var newArchive = CreateNewArchive(file, true, RelativePath);
                                newArchive.ParentArchive = this;
                            }
                            else
                            {
                                var newFile = DPFile.CreateNewFile(file, null);
                                newFile.AssociatedArchive = this;
                            }
                        }
                    }

                }
            }
        }

        internal override void ReadContentFiles()
        {
            throw new System.NotImplementedException();
        }

        internal override void ReadMetaFiles()
        {
            throw new System.NotImplementedException();
        }

        private Process Setup7ZProcess() {
            Process process = new Process();
            process.StartInfo.FileName = "7za.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            
            if (mode == Mode.Peek)
                process.StartInfo.ArgumentList.Add("l");
            process.StartInfo.ArgumentList.Add("-slt"); // Show technical information.
            if (IsInnerArchive)
                process.StartInfo.ArgumentList.Add(ExtractedPath);
            else
                process.StartInfo.ArgumentList.Add(Path);

            return process;
        }

        private bool GetMessage(Process process, out string msg) {
            process.Start();
            process.WaitForExit();
            msg = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(msg)) return false;
            return true;
        }

        private bool GetContents(string msg, out string[] contents) {
            const string fileInfoHeader = "----------\r\n";
            // Only spits out files, not folders.
            if (msg.Contains(fileInfoHeader))
            {
                // Then we got work to do.
                var contentBlocks = GetFileInfo(msg.Substring(msg.IndexOf(fileInfoHeader) + fileInfoHeader.Length));
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

        private string[] GetFileInfo(string msg) {
            Span<string> info = msg.Split("\r\n\r\n");
            info.Slice(0, info.Length - 1);
            return info.ToArray();
        }

        private bool CheckForErrors(string msg, out string errorMsg) {
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

        internal bool GetMultiParts(out string[] otherArchiveNames)
        {
            // Since the inital call did not throw an error, we can assume that there are valid multipart names.
            var similarFiles = Directory.GetFiles(IOPath.GetDirectoryName(ExtractedPath),
                                         IOPath.GetFileNameWithoutExtension(ExtractedPath));
            var numList = new List<int>(similarFiles.Length);
            var possibleArchiveNames = new List<string>(similarFiles.Length);
            foreach (var file in similarFiles)
            {
                var ext = GetExtension(file); // 0001
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
    }
}