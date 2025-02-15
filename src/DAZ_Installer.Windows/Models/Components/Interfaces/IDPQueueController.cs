using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// Controls the Queue Tab for the extract page.
    /// </summary>
    public interface IDPQueueController
    {
        /// <summary>
        /// Adds a job to the queue.
        /// </summary>
        /// <param name="job">The job to add.</param>
        public void AddJob(IDPExtractJob job);
        /// <summary>
        /// Removes all jobs from the queue.
        /// </summary>
        public void Clear();
        /// <summary>
        /// Updates the view associated with the job.
        /// </summary>
        /// <param name="job">The associated job to update the view for.</param>
        public void UpdateView(IDPExtractJob job);
        /// <summary>
        /// Updates the view associated with the job and archive info.
        /// </summary>
        /// <param name="job">The associated job to update the view for.</param>
        /// <param name="info">The associated archive info to update the view for.</param>
        public void UpdateView(IDPExtractJob job, DPArchiveInfo info);

        #region Event Handlers
        /// <summary>
        /// Handles the context menu event.
        /// </summary>
        /// <param name="sender">The object that is emitting the event, typically a <see cref="System.Windows.Forms.ListView"/>.</param>
        /// <param name="e">The cancel event args.</param>
        public void OnContextMenu(object sender, CancelEventArgs e);
        /// <summary>
        /// Handles the cancel job context menu event.
        /// </summary>
        public void OnCancelJob();
        /// <summary>
        /// Handles the cancel current archive context menu event.
        /// </summary>
        public void OnCancelCurrentArchive();
        /// <summary>
        /// Handles the skip archive context menu event.
        /// </summary>
        public void OnSkipArchive();
        /// <summary>
        /// Handles the view hierarchy context menu event.
        /// </summary>
        public void OnViewHierachy();
        /// <summary>
        /// Handles the view file list context menu event.
        /// </summary>
        public void OnViewFileList();
        #endregion
    }
}
