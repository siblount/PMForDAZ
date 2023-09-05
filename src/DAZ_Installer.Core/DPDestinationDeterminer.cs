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
        public override HashSet<DPFile> DetermineDestinations(DPArchive arc, DPProcessSettings settings)
        {
            // Handle Manifest first if a combo of both.
            var filesToExtract = new HashSet<DPFile>(arc.Contents.Count);
            if (settings.InstallOption == InstallOptions.ManifestOnly || settings.InstallOption == InstallOptions.ManifestAndAuto) 
                DetermineFromManifests(arc, settings, filesToExtract);

            if (settings.InstallOption == InstallOptions.ManifestOnly) return filesToExtract;

            // Prepare for file sense.
            DetermineContentFolders(arc, settings);
            UpdateRelativePaths(arc, settings);

            // Determine via file sense next.
            DetermineViaFileSense(arc, settings, filesToExtract);
            return filesToExtract;
        }

        private void DetermineContentFolders(DPArchive arc, in DPProcessSettings settings)
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
            var filePathPart = !string.IsNullOrEmpty(overridePath) ? overridePath : file.RelativeTargetPath;

            if (file.Parent is null || !settings.ContentRedirectFolders!.ContainsKey(Path.GetFileName(file.Parent.Path)))
                return Path.Combine(saveToTemp ? tmpLocation : settings.DestinationPath, filePathPart);

            return Path.Combine(saveToTemp ? tmpLocation : settings.DestinationPath,
                file.RelativeTargetPath ?? file.Parent.CalculateChildRelativeTargetPath(file, settings));
        }

        private void DetermineFromManifests(DPArchive arc, in DPProcessSettings settings, HashSet<DPFile> filesToExtract)
        {
            if (settings.InstallOption != InstallOptions.ManifestAndAuto && settings.InstallOption != InstallOptions.ManifestOnly) return;
            foreach (DPDSXFile? manifest in arc!.ManifestFiles.Where(f => f.Extracted))
            {
                Dictionary<string, string> manifestDestinations = manifest.GetManifestDestinations();

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

                    //else
                    //{
                    //    file.WillExtract = settingsToUse.InstallOption != InstallOptions.ManifestOnly;
                    //}
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
    }
}
