using DAZ_Installer.IO;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    public class DPDestinationDeterminer : AbstractDestinationDeterminer
    {
        protected override ILogger Logger { get; set; } = Log.Logger.ForContext<DPDestinationDeterminer>();

        public DPDestinationDeterminer() { }
        public DPDestinationDeterminer(ILogger logger) : base(logger) { }

        public override HashSet<DPFile> DetermineDestinations(DPArchive arc, DPProcessSettings settings)
        {
            // Handle Manifest first if a combo of both.
            var filesToExtract = new HashSet<DPFile>(arc.Contents.Count);
            HashSet<DPFolder> contentFolders = new HashSet<DPFolder>(4);
            List<Dictionary<string, string>>? destinations = null;
            if (settings.InstallOption == InstallOptions.ManifestOnly || settings.InstallOption == InstallOptions.ManifestAndAuto)
                contentFolders = SetContentFoldersFromManifest(arc, out destinations);
            if (settings.InstallOption == InstallOptions.ManifestAndAuto || settings.InstallOption == InstallOptions.Automatic)
                DetermineContentFolders(arc, contentFolders, settings);
            UpdateRelativePaths(arc, settings);

            if (settings.InstallOption != InstallOptions.ManifestOnly)
                DetermineViaFileSense(arc, settings, filesToExtract);

            // Determine manifest after file sense if manifest and auto. This way the manifest can override the file sense destinations.
            if (settings.InstallOption == InstallOptions.ManifestOnly || settings.InstallOption == InstallOptions.ManifestAndAuto)
                DetermineFromManifests(arc, destinations, settings, filesToExtract);
            return filesToExtract;
        }

        private HashSet<DPFolder> SetContentFoldersFromManifest(DPArchive arc, out List<Dictionary<string, string>> destinations)
        {
            HashSet<DPFolder> contentFolders = new HashSet<DPFolder>(4);
            destinations = new List<Dictionary<string, string>>(arc.ManifestFiles.Count);
            foreach (var manifest in arc.ManifestFiles.Where(x => x.Extracted))
            {
                try
                {
                    var dest = manifest.GetManifestDestinations();
                    destinations.Add(dest);
                    // The first folder after the "content" folder is the content folder.
                    foreach (var path in dest.Values)
                    {
                        var rootDir = PathHelper.GetRootDirectory(path, true);
                        if (string.IsNullOrEmpty(rootDir)) continue;
                        var result = arc.Folders.TryGetValue(PathHelper.NormalizePath(Path.Combine("Content", rootDir)), out var folder);
                        if (!result)
                        {
                            Logger.Warning("Could not find content folder {0} that was defined in manifest", rootDir, arc.FileName);
                            continue;
                        }
                        contentFolders.Add(folder);
                        folder.IsContentFolder = true;
                    }
                } catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to get manifest destinations for {0}", manifest.FileName);
                }
            }
            return contentFolders;
        }

        // TODO: Update this function to determine content folders from the manifest.
        // Or make this exclusive to auto mode.
        private void DetermineContentFolders(DPArchive arc, HashSet<DPFolder> ignoreSet, in DPProcessSettings settings)
        {
            // A content folder is a folder whose name is contained in the user's common content folders list
            // or in their folder redirects map.


            // Prepare sort so that the first elements in folders are the ones at root.
            DPFolder[] folders = arc.Folders.Values.ToArray();
            var foldersKeys = new byte[folders.Length];

            for (var i = 0; i < foldersKeys.Length; i++)
            {
                foldersKeys[i] = PathHelper.GetSubfoldersCount(folders[i].Path);
            }

            // Elements at the beginning are folders at root levels.
            Array.Sort(foldersKeys, folders);

            foreach (DPFolder? folder in folders)
            {
                if (ignoreSet.Contains(folder)) continue;
                var folderName = Path.GetFileName(folder.Path);
                var elgibleForContentFolderStatus = settings.ContentFolders.Contains(folderName) ||
                                                    settings.ContentRedirectFolders.ContainsKey(folderName);
                if (folder.Parent is null)
                    folder.IsContentFolder = elgibleForContentFolderStatus;
                else
                {
                    if (folder.Parent.IsContentFolder || folder.Parent.IsPartOfContentFolder) continue;
                    folder.IsContentFolder = elgibleForContentFolderStatus;
                }
            }
        }
        /// <summary>
        /// Updates the relative paths of all files and folders in the archive relative to the content folder.
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        /// <param name="settings">The settings to use.</param>
        private void UpdateRelativePaths(DPArchive arc, in DPProcessSettings settings)
        {
            foreach (DPFile content in arc.RootContents)
                content.RelativePathToContentFolder = content.RelativeTargetPath = content.Path;
            foreach (DPFolder folder in arc.Folders.Values)
                folder.UpdateChildrenRelativePaths(settings);
        }

        /// <summary>
        /// This function returns the target path based on whether it is saving to it's destination or to a
        /// temporary location, whether the <paramref name="file"/> has a relative path or not, and whether
        /// the file's parent is in folderRedirects. <para/>
        /// Additionally, there is <paramref name="overridePath"/> which will be used for combining paths publicly;
        /// <b>however</b>, this will be ignored if the parent name is in the user's folder redirects.
        /// </summary>
        /// <param name="file">The file to get a target path for.</param>
        /// <param name="settings">The process settings to use.</param>
        /// <param name="saveToTemp">Determines whether to get a target path saving to a temporary location.</param>
        /// <param name="overridePath">The path to combine with instead of usual combining. </param>
        /// <returns>The target path for the specified file. </returns>
        private string GetTargetPath(DPFile file, in DPProcessSettings settings, bool saveToTemp = false, string? overridePath = null)
        {
            var tmpLocation = Path.Combine(settings.TempPath, "DazProductInstaller");
            // file.RelativeTargetPath already substituted the content folder name with the redirect folder name.
            var filePathPart = !string.IsNullOrEmpty(overridePath) ? overridePath : file.RelativeTargetPath;

            if (filePathPart is null) Log.Warning("GetTargetPath() filePathPart is null.");

            if (file.Parent is null)
                return Path.Combine(saveToTemp ? tmpLocation : settings.DestinationPath, filePathPart);

            var contentFolderName = GetContentFolderManifestString(file.Path);
            var hasRedirect = settings.ContentRedirectFolders!.ContainsKey(contentFolderName);
            // We need to 
            if (overridePath is not null && hasRedirect)
                return Path.Combine(settings.DestinationPath, settings.ContentRedirectFolders[contentFolderName], filePathPart.Remove(0, contentFolderName.Length + 1));

            // If the installation method includes Automatic, then relative target path calculations have been done prior to this function.
            // Therefore, we can just use the relative target path.
            return Path.Combine(saveToTemp ? tmpLocation : settings.DestinationPath,
                filePathPart ?? file.Parent.CalculateChildRelativeTargetPath(file, settings));
        }

        private void DetermineFromManifests(DPArchive arc, List<Dictionary<string, string>>? dests, in DPProcessSettings settings, HashSet<DPFile> filesToExtract)
        {
            if (dests is null) dests = arc!.ManifestFiles.Select(f => f.GetManifestDestinations()).ToList();
            if (settings.InstallOption != InstallOptions.ManifestAndAuto && settings.InstallOption != InstallOptions.ManifestOnly) return;
            foreach (var manifestDestinations in dests)
            {
                foreach (DPFile file in arc.Contents.Values)
                {
                    try
                    {
                        if (!manifestDestinations.ContainsKey(file.Path) || filesToExtract.Contains(file)) continue;
                        file.TargetPath = GetTargetPath(file, settings, overridePath: manifestDestinations[file.Path]);
                        filesToExtract.Add(file);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to determine file to extract: {0}", file.Path);
                        Logger.Debug("File information: {@0}", file);
                    }
                }
            }
        }

        private void DetermineViaFileSense(DPArchive arc, in DPProcessSettings settings, HashSet<DPFile> filesToExtract)
        {

            if (settings.InstallOption != InstallOptions.Automatic && settings.InstallOption != InstallOptions.ManifestAndAuto) return;
            // Get contents where file was not extracted.
            Dictionary<string, DPFolder>.ValueCollection folders = arc.Folders.Values;

            foreach (DPFolder folder in folders)
            {
                if (!folder.IsContentFolder && !folder.IsPartOfContentFolder) continue;
                // Update children's relative path.
                folder.UpdateChildrenRelativePaths(settings);

                foreach (DPFile child in folder.Contents)
                {
                    //Get destination path and update child destination path.
                    child.TargetPath = GetTargetPath(child, settings);

                    filesToExtract.Add(child);
                }
            }
            // Now hunt down all files in folders that aren't in content folders.
            foreach (DPFolder folder in folders)
            {
                if (folder.IsContentFolder) continue;
                // Add all archives to the inner archives to process for later processing.
                foreach (DPFile file in folder.Contents)
                {
                    if (file is not DPArchive nestedArc) continue;
                    arc.TargetPath = GetTargetPath(nestedArc, settings, true);
                    // Add to queue.
                    arc.Subarchives.Add(nestedArc);
                    filesToExtract.Add(nestedArc);
                }
            }

            // Hunt down all files in root content.

            foreach (DPFile content in arc.RootContents)
            {
                if (content is not DPArchive nestedArc) continue;
                nestedArc.TargetPath = GetTargetPath(nestedArc, settings, true);
                // Add to queue.
                arc.Subarchives.Add(nestedArc);
                filesToExtract.Add(nestedArc);
            }
        }

        /// <summary>
        /// A function that determines the content folder of the archive based off of the manifest.
        /// It returns the folder name after the "Content/" folder in the manifest in the <paramref name="path"/>.
        /// </summary>
        /// <param name="path"> The path that the manifest has for a file. </param>
        /// <returns>The content folder of the archive.</returns>
        private string GetContentFolderManifestString(string path)
        {
            var segments = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 1 ? segments[1] : string.Empty;
        }
    }
}
