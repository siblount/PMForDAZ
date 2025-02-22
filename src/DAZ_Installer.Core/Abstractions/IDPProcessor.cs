using DAZ_Installer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Defines the contract for processing DAZ Studio Product archives. This interface represents
    /// a high-level orchestrator that manages the entire archive processing workflow, including
    /// extraction, analysis, and installation of DAZ Studio products.
    /// </summary>
    /// <remarks>
    /// The processor is responsible for:
    /// <list type="bullet">
    ///     <item>Orchestrating the extraction and processing of DAZ Studio Product archives</item>
    ///     <item>Managing the workflow of discovering, analyzing, and installing product files</item>
    ///     <item>Determining which files should be extracted and where they should be installed</item>
    ///     <item>Coordinating with various components like extractors, metadata readers, and tag providers</item>
    ///     <item>Providing progress updates and error handling through events</item>
    /// </list>
    /// </remarks>
    public interface IDPProcessor
    {
        /// <summary>
        /// An event that is invoked when a file that is being extracted, moved, or deleted throws an error.
        /// <seealso cref="ProcessError"/>
        /// </summary>
        public event DPProcessorEventHandler<DPArchiveErrorArgs>? FileError;
        /// <summary>
        /// Occurs when the processor has encountered an recoverable (or unrecoverable) error.
        /// </summary>
        public event DPProcessorEventHandler<DPProcessorErrorArgs>? ProcessError;
        /// <summary>
        /// Occurs when the processor has about to begin processing an archive.
        /// </summary>
        /// <remarks>
        /// For every ArchiveEnter innovcation, <see cref="ArchiveExit"/> is guaranteed to be called
        /// for the same archive.
        /// </remarks>
        public event DPProcessorEventHandler<DPArchiveEnterArgs>? ArchiveEnter;
        /// <summary>
        /// Occurs when the processor has finished processing an archive.
        /// </summary>
        /// <remarks>
        /// For every <see cref="ArchiveEnter"/> innovcation, ArchiveExit is guaranteed to be called
        /// for the same archive.
        /// </remarks>
        public event DPProcessorEventHandler<DPArchiveExitArgs>? ArchiveExit;
        /// <summary>
        /// Occurs when the extractor has emitted a progress update.
        /// </summary>
        /// <remarks>
        /// It should not show the extraction of each individual file, 
        /// but rather the overall progress of the extraction. In other words,
        /// reporting 1% should report 1% of the archive, not 1% of the file 
        /// being extracted.
        /// </remarks>
        public event DPProcessorEventHandler<DPExtractProgressArgs>? ExtractProgress;
        /// <summary>
        /// Occurs when the extractor has emitted a move progress update.
        /// </summary>
        /// <remarks>
        /// This could occur when, for example, an extractor must extract all the files to temp first, 
        /// then move them to the final destination.
        /// </remarks>
        public event DPProcessorEventHandler<DPExtractProgressArgs>? MoveProgress;
        /// <summary>
        /// Occurs when the processor has finished processing all archives
        /// (or after an error or cancellation).
        /// </summary>
        /// <remarks>
        /// When the event is emitted, the <see cref="State"/> should be
        /// <see cref="ProcessorState.Idle"/>.
        /// </remarks>
        public event Action? Finished;
        /// <summary>
        /// Occurs when the <see cref="State"/> property has changed.
        /// </summary>
        public event Action? StateChanged;
        /// <summary>The current state of the processor.</summary>
        /// <remarks>Invokes <see cref="StateChanged"/> when state is changed.</remarks>
        public ProcessorState State { get; }
        /// <summary>
        /// The current archive that is being processed.
        /// </summary>
        /// <value>
        /// The current archive that is being processed or null 
        /// if the Processor is in <see cref="ProcessorState.Idle"/>
        /// </value>
        public IDPArchive? CurrentArchive { get; }
        /// <summary>
        /// The current process settings being used by the processor.
        /// </summary>
        /// <remarks>
        /// Check if the <see cref="State"/> is not <see cref="ProcessorState.Idle"/>
        /// before accessing this property.
        /// </remarks>
        public DPProcessSettings CurrentProcessSettings { get; }
        /// <summary>
        /// Begin processing the archive at location
        /// </summary>
        /// <param name="filePath">The file path of the archive to process.</param>
        /// <param name="settings">The process settings to use.</param>

        public void ProcessArchive(string filePath, DPProcessSettings settings);
        /// <summary>
        /// Cancels the processing of the archive and any pending archives.
        /// </summary>
        /// <remarks>
        /// Pending nested archives will not be processed at all after calling this function.
        /// It is still possible for some processing for the current archive being processed, but never
        /// the next archives.
        /// </remarks>
        public void CancelProcessing();
        /// <summary>
        /// Cancels only the current archive.
        /// </summary>
        /// <remarks>
        /// Requests for cancellation for the current archive. This will not abruptly stop the processing, 
        /// but rather request the current archive to stop processing. It is possible that an archive is
        /// processed even after cancellation is requested.
        /// </remarks>
        public void CancelCurrentArchive();
    }
}
