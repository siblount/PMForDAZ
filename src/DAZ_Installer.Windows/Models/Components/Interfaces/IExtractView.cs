using DAZ_Installer.Core;
using DAZ_Installer.UI;
using DAZ_Installer.Windows.DP;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// Represents the view interface for the extraction process, handling the display and management of archive extraction jobs.
    /// </summary>
    public interface IExtractView : IControl
    {
        /// <summary>
        /// Adds a new extraction job to the queue and updates the UI accordingly.
        /// </summary>
        /// <param name="job">The extraction job to be added to the queue.</param>
        /// <remarks>
        /// This method should be thread-safe and handle cross-thread UI updates appropriately.
        /// The job will be displayed in the queue view and its status will be tracked.
        /// </remarks>
        void AddToQueue(DPExtractJob job);

        /// <summary>
        /// Adds an archive to the list view component of the UI.
        /// </summary>
        /// <param name="archive">The archive to be added to the list.</param>
        /// <remarks>
        /// This method should update the list view to show the archive's basic information
        /// such as name, size, and status.
        /// </remarks>
        void AddToList(IDPArchive archive);

        /// <summary>
        /// Adds an archive to the hierarchical view component of the UI.
        /// </summary>
        /// <param name="workingArchive">The archive to be added to the hierarchy.</param>
        /// <remarks>
        /// This method should update the tree view to show the archive's structure,
        /// including any nested archives and their relationships.
        /// </remarks>
        void AddToHierachy(IDPArchive workingArchive);

        /// <summary>
        /// Resets the extract page to its initial state.
        /// </summary>
        /// <remarks>
        /// This method should clear all lists, reset progress indicators,
        /// and prepare the UI for a new extraction session.
        /// </remarks>
        void ResetExtractPage();

        /// <summary>
        /// Opens the file list tab for an archive. 
        /// </summary>
        /// <param name="archive">The archive to show the file list for.</param>
        void ShowFileListTab(IDPArchive archive);

        /// <summary>
        /// Opens the file list tab for an archive. 
        /// </summary>
        /// <param name="archive">The archive to show the file hierachy for.</param>
        void ShowFileHierachyTab(IDPArchive archive);

        /// <summary>
        /// Updates the status of an existing extraction job in the UI.
        /// </summary>
        /// <param name="caller">The extraction job that triggered the update.</param>
        /// <param name="info">Archive information containing the current status and details.</param>
        /// <remarks>
        /// This method is called when an archive's status changes during processing.
        /// It should update relevant UI elements to reflect the current state of the archive.
        /// </remarks>
        void OnExtractJobStatusUpdate(DPExtractJob caller, DPArchiveInfo info);

        /// <summary>
        /// Handles the <see cref="IDPProcessor.ExtractProgress"/> event.
        /// </summary>
        /// <param name="processor">The processor that emitted the event</param>
        /// <param name="e">The event args</param>
        void OnExtractionProgressUpdate(IDPProcessor processor, DPExtractProgressArgs e);

        /// <summary>
        /// Handles the <see cref="IDPProcessor.MoveProgress"/> event.
        /// </summary>
        /// <param name="processor">The processor that emitted the event</param>
        /// <param name="e">The event args</param>
        void OnMoveProgressUpdate(IDPProcessor processor, DPExtractProgressArgs e);

        /// <summary>
        /// Updates the GUI to represent that the <see cref="IDPProcessor"/> is
        /// preparing to extract contents.
        /// </summary>
        /// <param name="processor">The processor whose state has changed.</param>
        void OnProcessorStateUpdate(IDPProcessor processor);
        
        /// <summary>
        /// Updates the GUI to represent that a record is being created for the archive.
        /// </summary>
        /// <param name="archive">The archive whose records is being created for.</param>
        void OnCreatingRecords(IDPArchive archive);
        /// <summary>
        /// Updates the GUI when the processor has finished.
        /// </summary>
        /// <param name="job">The job that has been completed.</param>
        void OnProcessorFinished(IDPExtractJob job);
    }
}
