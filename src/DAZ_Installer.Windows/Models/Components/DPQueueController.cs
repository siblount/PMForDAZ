using DAZ_Installer.IO;
using DAZ_Installer.UI;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// 
    /// </summary>
    public class DPQueueController(IListView listView,
                                   IExtractView extractView,
                                   ToolStripMenuItem viewHierachyItem,
                                   ToolStripMenuItem viewFileListItem,
                                   ToolStripSeparator viewSeperator,
                                   ToolStripMenuItem cancelJobItem,
                                   ToolStripMenuItem cancelCurrentJobItem,
                                   ToolStripMenuItem skipArchiveItem,
                                   ToolStripMenuItem cancelCurrentArchiveItem,
                                   ImageList statusIcons) : IDPQueueController
    {
        private readonly IListView queueListView = listView;
        /// <summary>
        /// The logger to use.
        /// </summary>
        /// <returns>By default, <see cref="Log"/> with <see cref="DPQueueController"/> context.</returns>
        public ILogger Logger { get; set; } = Log.ForContext<DPQueueController>();
        internal static List<IDPExtractJob> extractJobs = new(4);
        private static readonly Dictionary<string, QueueItem> associatedQueueItems = new(64, PathComparer.Instance);
        private static bool IsAllJobsFinished => extractJobs.Count is 0 || extractJobs.All(job => job.TaskJob is null or { Status: TaskStatus.RanToCompletion or TaskStatus.Canceled or TaskStatus.Faulted });

        private struct QueueItem
        {
            public ListViewItem associatedListItem;
            public IDPExtractJob associatedJob;
            public string archiveInfoKey;

            public QueueItem(ListViewItem item, IDPExtractJob job, string key) => 
                (associatedListItem, associatedJob, archiveInfoKey) = (item, job, key);
        }

        /// <inheritdoc/>
        public void AddJob(IDPExtractJob job)
        {
            if (IsAllJobsFinished) Clear();

            // Ensure the final archive states are updated when the processor exits.
            job.Processor.Finished += () =>
            {
                if (queueListView.InvokeRequired) queueListView.BeginInvoke(() => UpdateView(job));
                else UpdateView(job);
            };

            extractJobs.Add(job);
            var letter = (char)('A' - 1 + extractJobs.Count);

            queueListView.BeginUpdate();
            try
            {
                var groupKey = job.GetHashCode().ToString();
                queueListView.Groups.Add(groupKey, $"Extract Job {letter}");
                foreach (string file in job.InitialFilesToProcess)
                {
                    var normalizedFilePath = PathHelper.NormalizePath(file);
                    var visibleArchiveName = PathHelper.GetFileName(normalizedFilePath);
                    ListViewItem item = queueListView.Items.Add(visibleArchiveName);
                    var queueItem = new QueueItem(item, job, normalizedFilePath);
                    associatedQueueItems.Add(normalizedFilePath, queueItem);
                    item.Tag = queueItem;
                    item.ToolTipText = "This archive is in the queue because it was manually added to the queue.";
                    item.Group = queueListView.Groups[groupKey];
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occured while adding an extract job to the queue");
            }
            finally
            {
                queueListView.EndUpdate();
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            extractJobs.Clear();
            associatedQueueItems.Clear();
            ClearExtractQueue();
            GC.Collect();
        }

        /// <inheritdoc/>
        public void UpdateView(IDPExtractJob job)
        {
            queueListView.BeginUpdate();
            try
            {
                var archiveInfosSnapshot = job.GetArchiveInfosSnapshot();
                foreach (var info in archiveInfosSnapshot.Values)
                {
                    UpdateQueueItem(job, info);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occured when attempting to update all archive infos");
            }
            finally
            {
                queueListView.EndUpdate();
            }
        }

        /// <inheritdoc/>
        public void UpdateView(IDPExtractJob job, DPArchiveInfo info)
        {
            queueListView.BeginUpdate();
            try
            {
                UpdateQueueItem(job, info);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occured when handling extract job status update");
            }
            finally
            {
                queueListView.EndUpdate();
            }
        }

        /// <inheritdoc/>
        private void ClearExtractQueue()
        {
            queueListView.BeginUpdate();
            try
            {
                queueListView.Items.Clear();
                queueListView.Groups.Clear();
            }
            finally
            {
                queueListView.EndUpdate();
            }
        }

        /// <inheritdoc/>
        public void OnCancelCurrentArchive()
        {
            // Find the one that is currently processing.
            foreach (var job in extractJobs)
            {
                if (job.TaskJob is null || job.TaskJob.IsCompleted) continue;
                job.CancelCurrentArchive();
                return;
            }
            Logger.Warning("No job was found to cancel the current archive for");
        }

        /// <inheritdoc/>
        public void OnCancelJob()
        {
            // Get the selected items and find their associated jobs and cancel them.
            queueListView.SelectedItems.OfType<ListViewItem>().Select(item => item.Tag).OfType<QueueItem>().Select(queueItem => queueItem.associatedJob).Distinct().ToList().ForEach(job => job.CancelJob());
        }

        /// <inheritdoc/>
        public void OnContextMenu(object _, CancelEventArgs e)
        {
            // If there are no jobs have ever run (aka: app just started), disable all context menu items.
            if (extractJobs.Count is 0)
            {
                e.Cancel = true;
                return;
            }

            // Hide elements that are only for selected items.
            if (queueListView.SelectedItems.Count is 0)
            {
                viewHierachyItem.Visible = viewFileListItem.Visible = viewSeperator.Visible =
                skipArchiveItem.Enabled = false;
            }
            else if (queueListView.SelectedItems.Count is 1)
            {
                // Get the selected item, if it has files, then enable view options.
                // If it is completed or scheduled for cancellation, disable the skip archive button.
                if (queueListView.SelectedItems[0].Tag is QueueItem item)
                {
                    if (item.associatedJob.GetArchiveInfosSnapshot().TryGetValue(item.archiveInfoKey, out var info))
                    {
                        viewHierachyItem.Enabled = viewFileListItem.Enabled = info.Archive?.Contents.Count is not 0;
                        skipArchiveItem.Enabled = !IsCompletedOrScheduledForCancellation(info);
                    } else Logger.Error("Could not check archive status for context menu; Extract job snapshot did not contain archive info for key: {key}", item.archiveInfoKey);
                } else Logger.Error("The selected item did not have an QueueItem tag set for item: {item}", queueListView.SelectedItems[0].Text);
                viewHierachyItem.Visible = viewFileListItem.Visible = viewSeperator.Visible = true;
            } else
            {
                viewHierachyItem.Visible = viewFileListItem.Visible = viewSeperator.Visible = false;
                skipArchiveItem.Enabled = true;
            }

            // Only show cancel current archive or job if there is a running job.
            cancelCurrentArchiveItem.Enabled = cancelJobItem.Enabled = cancelCurrentJobItem.Enabled =
                extractJobs.Any(job => job.TaskJob is not null && job.TaskJob.Status is TaskStatus.Running);
        }

        /// <inheritdoc/>
        public void OnSkipArchive()
        {
            foreach (ListViewItem item in queueListView.SelectedItems) {
                if (item.Tag is QueueItem queueItem) {
                    try {
                        queueItem.associatedJob.SkipArchive(queueItem.archiveInfoKey);
                    } catch (ArgumentException argEx) {
                        Logger.Error(argEx, @"Failed to skip archive because the key was not found in
                         the associated job for item: {}", item.Text);
                    }
                } else {
                    Logger.Error("Failed to skip archive due to missing/unexpected Tag for item: {}", item.Text);
                }
            }
        }

        /// <inheritdoc/>
        public void OnViewFileList()
        {
            if (queueListView.SelectedItems.Count is not 1) {
                Logger.Warning("OnViewFileList should not have been called for a count that is not 1");
                return;
            }

            if (queueListView.SelectedItems[0].Tag is QueueItem item) {
               if (item.associatedJob.GetArchiveInfosSnapshot().TryGetValue(item.archiveInfoKey, out var archiveInfo)) {
                if (archiveInfo.Archive is not null) 
                    extractView.ShowFileListTab(archiveInfo.Archive);
               } else {
                Logger.Error("Cannot view file list because archive info snapshot did not contain the key associated with the list item for item: {item}",
                    queueListView.SelectedItems[0].Text);
               }
            } else Logger.Error("Cannot view file list due to a missing/unexpected Tag for item: {item}", 
                queueListView.SelectedItems[0].Text);
        }

        /// <inheritdoc/>
        public void OnViewHierachy()
        {
            if (queueListView.SelectedItems.Count is not 1) {
                Logger.Warning("OnViewHierachy should not have been called for a count that is not 1");
                return;
            }

            if (queueListView.SelectedItems[0].Tag is QueueItem item) {
               if (item.associatedJob.GetArchiveInfosSnapshot().TryGetValue(item.archiveInfoKey, out var archiveInfo)) {
                if (archiveInfo.Archive is not null) 
                    extractView.ShowFileListTab(archiveInfo.Archive);
               } else {
                Logger.Error("Cannot view file list because archive info snapshot did not contain the key associated with the list item for item: {item}",
                    queueListView.SelectedItems[0].Text);
               }
            } else Logger.Error("Cannot view file list due to a missing/unexpected Tag for item: {item}", 
                queueListView.SelectedItems[0].Text);
        }

        private void UpdateQueueItem(IDPExtractJob caller, DPArchiveInfo info)
        {
            var item = EnsureAssociatedQueueItem(caller, info);
            if (item is not null)
            {
                item.ForeColor = DetermineColorForArchive(info);
                item.StateImageIndex = DetermineImageIndexForArchive(info);
                DetermineErrorMessage(info, item);
            }
            else Logger.Error("Failed to update associated queue item due to null queue item for arc: {arc}", info.FilePath);
        }

        private ListViewItem? EnsureAssociatedQueueItem(IDPExtractJob caller, DPArchiveInfo info)
        {
            if (associatedQueueItems.TryGetValue(info.FilePath, out var item)) return item.associatedListItem;
            try
            {
                queueListView.BeginUpdate();
                var visibleArchiveName = GetVisibleArchiveName(info);

                var listItem = queueListView.Items.Add(visibleArchiveName);

                item = new QueueItem(listItem, caller, info.FilePath);
                listItem.Tag = item;
                associatedQueueItems.Add(info.FilePath, item);

                SetTooltipMessageForArchiveQueueListViewItem(info, item.associatedListItem);
                try
                {
                    listItem.Group = queueListView.Groups[caller.GetHashCode().ToString()];
                }
                catch
                {
                    Logger.Error("Queue list view group key {} does not exist", caller.GetHashCode().ToString());
                }
                return listItem;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to esnure associated queue item for job {@job} with info {@info}", caller, info);
            }
            finally
            {
                queueListView.EndUpdate();
            }
            return null;
        }

        private static void SetTooltipMessageForArchiveQueueListViewItem(DPArchiveInfo info, ListViewItem item)
        {
            if (info is { Archive.IsInnerArchive: true })
                item.ToolTipText = "This archive is in the queue because the processor deemed it necessary to process it. " +
                                   (info.Archive is { AssociatedArchive: { } } ? $"The parent archive is {info.Archive.AssociatedArchive.FileName}." : "");
            else item.ToolTipText = "This archive is in the queue because it was manually added to the queue.";
        }

        /// <summary>
        /// Gets the visible name of the archive.
        /// </summary>
        /// <remarks>
        /// This is for the extract queue list view. This will show the file name of the archive and if it is an inner archive, 
        /// it will show the parent archive name. This will prove as an explanation for why an archive is in the queue.
        /// </remarks>
        /// <param name="info">The archive info to get a name for.</param>
        /// <returns>The file name of the archive if it is a parent archive, 
        /// otherwise the the file name with the parent file name mentioned.
        /// </returns>
        private static string GetVisibleArchiveName(DPArchiveInfo info)
        {
            var fileName = PathHelper.GetFileName(info.FilePath);
            var parentName = info is { Archive.AssociatedArchive: not null } ? PathHelper.GetFileName(info.Archive.AssociatedArchive.FileName) : string.Empty;
            return info.Archive is { IsInnerArchive: true } ? $"{fileName} (from {parentName})" : fileName;
        }

        private static void DetermineErrorMessage(DPArchiveInfo info, ListViewItem item)
        {
            var subItem = item.SubItems.Count == 2 ? item.SubItems[1] : new ListViewItem.ListViewSubItem(item, "");
            switch (info.Status)
            {
                case DPArchiveStatus.CompletedWithIssues:
                    subItem.Text = info.Errors.Count == 0 ? "Completed but some files were not extracted" : "Completed with errors";
                    break;
                case DPArchiveStatus.Failed:
                    if (info.Errors.Count == 0)
                        subItem.Text = "Failed to extract";
                    else if (info.Errors.Count >= 2)
                        subItem.Text = "Multiple errors occurred";
                    else if (info.Errors[0].Exception is not null || info.Errors[0].Explanation is not null)
                        subItem.Text = info.Errors[0].Exception?.Message ?? info.Errors[0].Explanation;
                    else
                        subItem.Text = "Failed to extract due to an unknown error";
                    break;
                case DPArchiveStatus.Cancelled:
                    if (info.Errors.Count != 0)
                        subItem.Text = "Cancelled with errors";
                    break;
            }
        }

        private static Color DetermineColorForArchive(DPArchiveInfo info)
        {
            if (info.Status is DPArchiveStatus.Cancelled) return Color.Gray;
            if (info.Status is DPArchiveStatus.Failed) return Color.Red;
            if (info.Status is DPArchiveStatus.Completed) return Color.Green;
            if (info.Status is DPArchiveStatus.CompletedWithIssues) return Color.Orange;
            if (info.Archive is { IsInnerArchive: true }) return Color.Blue;
            return Color.Black;
        }

        private int DetermineImageIndexForArchive(DPArchiveInfo info)
        {
            return info.Status switch
            {
                DPArchiveStatus.Failed => statusIcons.Images.IndexOfKey("error"),
                DPArchiveStatus.CompletedWithIssues => statusIcons.Images.IndexOfKey("warning"),
                _ => -1
            };
        }

        private static bool IsCompletedOrScheduledForCancellation(DPArchiveInfo info) => 
            info.Status is DPArchiveStatus.Completed 
                        or DPArchiveStatus.CompletedWithIssues 
                        or DPArchiveStatus.Failed
                        or DPArchiveStatus.Cancelled 
                        or DPArchiveStatus.CancellationRequested;
    }
}
