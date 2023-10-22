using DAZ_Installer.IO;
using Serilog;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// An abstract class for all extractors.
    /// </summary>
    public abstract class DPAbstractExtractor
    {
        /// <summary>
        /// The logger to use for this extractor, if any.
        /// </summary>
        public virtual ILogger Logger { get; set; } = Log.Logger.ForContext<DPAbstractExtractor>();
        /// <summary>
        /// The context to use for moving and (potentially) extracting files. <para/>
        /// <b>WARNING: Context may not be used to the full extent.</b> For example, 7z requires all files be extracted to a temp location first, then moved to the final destination. <para/>
        /// However, moving files is guaranteed to use this context to it's full extent. WinZip and RAR files extract directly to the final destination, so the context is used to it's full extent.
        /// </summary>
        public AbstractFileSystem FileSystem { get; protected set; } = new DPFileSystem();
        /// <summary>
        /// The cancellation token to use for the extraction. By default, it is <see cref="CancellationToken.None"/>.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        /// <summary>
        /// The current mode of the archive file; describes whether the archive is peeking, extracting, or moving files.
        /// </summary>
        protected enum Mode
        {
            /// <summary>
            /// The archive is discovering files and folders.
            /// </summary>
            Peek,
            /// <summary>
            /// The archive is extracting files to a location.
            /// </summary>
            Extract,
            /// <summary>
            /// The archive is moving files from (usually) temp to its final destination.
            /// </summary>
            Moving
        }
        protected Mode mode;
        /// <summary>
        /// An event that is fired when an error occurs during extraction.
        /// </summary>
        public event DPArchiveEventHandler<DPArchiveErrorArgs>? ArchiveErrored;
        /// <summary>
        /// An event that is fired when the progress of extraction changes.
        /// </summary>
        public event DPArchiveEventHandler<DPExtractProgressArgs>? ExtractProgress;
        /// <summary>
        /// An event that is fired when the progress of moving the files (from temp to dest) changes.
        /// </summary>
        public event DPArchiveEventHandler<DPExtractProgressArgs>? MoveProgress;
        /// <summary>
        /// An event that is fired when the extractor is beginning to extract files, for some extractors, it may
        /// need to extract files to a temporary location first. Check the <see cref="Moving"/> event to know
        /// when the files are being moved to the final destination.
        /// </summary>
        public event Action? Extracting;
        /// <summary>
        /// An event that is fired when the extractor is beginning to peek files.
        /// </summary>
        public event Action? Peeking;
        /// <summary>
        /// An event that is fired when the extractor is moving files to the final destination. This event may not
        /// be invoked for some extractors. This event only occurs when the extractor is moving files from a temporary
        /// location.
        /// </summary>
        public event Action? Moving;
        /// <summary>
        /// An event that is fired when the extractor is finished peeking files.
        /// </summary>
        public event Action? PeekFinished;
        /// <summary>
        /// An event that is fired when the extractor is finished extracting files.
        /// </summary>
        public event Action? ExtractFinished;
        /// <summary>
        /// An event that is fired when the extractor is finished moving files.
        /// </summary>
        public event Action? MoveFinished;
        /// <summary>
        /// Attempts to extract files specifed in <see cref="DPExtractSettings.SourceToDestinationMap"/>. <br/>
        /// If the archive has no children, then the file will be peeked first via <see cref="Peek(DPArchive)"/>. <br/>
        /// After that, the files will be extracted and a report will be returned when finished.
        /// </summary>
        /// <param name="settings">The settings to use for extraction.</param>
        /// <returns>An extraction report indicating what files successfully extracted, what errored, etc.</returns>
        public abstract DPExtractionReport Extract(DPExtractSettings settings);
        /// <summary>
        /// An optimized version of <see cref="Extract(DPExtractSettings)"/> that extracts files to the temporary location defined
        /// by <see cref="DPExtractSettings.TempLocation"/>. It does <b>NOT</b> extract to <see cref="DPExtractSettings.DestinationPath"/>.<br/>
        /// If the archive has no children, then the file will be peeked first
        /// via <see cref="Peek(DPArchive)"/>. <br/> After that, the files will be extracted and a report will be returned when finished.
        /// </summary>
        /// <param name="settings">Settings to use for extraction.</param>
        /// <returns></returns>
        public abstract DPExtractionReport ExtractToTemp(DPExtractSettings settings);
        /// <summary>
        /// Checks to see if the archive exists (and accessible) and if it is a valid archive. It will create a new instance
        /// of <see cref="DPArchive"/> for you, <br/> peeks the archive through <see cref="Peek(DPArchive)"/> and attempts
        /// to extract via <see cref="Extract(DPExtractSettings)"/>. <br/>
        /// After that, the files will be extracted and a report will be returned when finished. <br/>
        /// <b>ADDITIONAL NOTE:</b> <see cref="DPExtractSettings.Archive"/> will be overwritten to the new archive that is created.
        /// </summary>
        /// <param name="settings">The settings to use for extraction.</param>
        /// <param name="archive">The archive that you wish to extract from.</param>
        /// <returns>An extraction report indicating what files successfully extracted, what errored, etc.</returns>
        public DPExtractionReport Extract(DPExtractSettings settings, IDPFileInfo archive)
        {
            ValidateArchive(archive);
            var arc = DPArchive.CreateNewParentArchive(archive);
            settings.Archive = arc;
            return Extract(settings);
        }
        /// <summary>
        /// Seeks the archive for it's contents and creates file objects and folders as children of <paramref name="archive"/>. <br/>
        /// You can check the contents found through <see cref="DPArchive.Contents"/>. <br/>
        /// </summary>
        /// <param name="archive">The archive you wish to seek files for.</param>
        public abstract void Peek(DPArchive archive);
        /// <summary>
        /// Checks to see if archive exists (and accessible) and if it is a valid archive. It will create a new
        /// instance of <see cref="DPArchive"/> for you, peeks the archive through <see cref="Peek(DPArchive)"/>,
        /// and returns the instance.
        /// </summary>
        /// <param name="archive">The archive that you wish to peek.</param>
        /// <returns>An archive object that is ready for extraction.</returns>
        public DPArchive Peek(IDPFileInfo archive)
        {
            ValidateArchive(archive);
            var arc = DPArchive.CreateNewParentArchive(archive);
            Peek(arc);
            return arc;
        }
        /// <summary>
        /// Validates the archive to make sure it exists and is a valid archive.
        /// </summary>
        /// <param name="archive">The archive that you wish to validate.</param>
        /// <exception cref="FileNotFoundException">The archive does not exist or application does not have access to it.</exception>
        /// <exception cref="ArgumentException">The file is not an 7z, rar, or winzip archive after checking its file signature.</exception>
        protected virtual void ValidateArchive(IDPFileInfo archive)
        {
            if (!archive.Exists) throw new FileNotFoundException("The archive file does not exist.", archive.Path);
            if (DPArchive.DetermineArchiveFormatPrecise(archive.OpenRead(), true) == ArchiveFormat.Unknown)
                throw new ArgumentException("The archive file is not a valid archive.", archive.Path);
        }
        /// <summary>
        /// Invoke the <see cref="Extracting"/> event.
        /// </summary>
        protected virtual void EmitOnExtracting() => Extracting?.Invoke();
        /// <summary>
        /// Invoke the <see cref="Peeking"/> event.
        /// </summary>
        protected virtual void EmitOnPeeking() => Peeking?.Invoke();
        /// <summary>
        /// Invoke the <see cref="Moving"/> event.
        /// </summary>
        protected virtual void EmitOnMoving() => Moving?.Invoke();
        /// <summary>
        /// Invoke the <see cref="ExtractProgress"/> event.
        /// </summary>
        /// <param name="arc">The archive whose progress has changed.</param>
        /// <param name="args">The extraction args.</param>
        protected virtual void EmitOnExtractionProgress(DPArchive arc, DPExtractProgressArgs args) => ExtractProgress?.Invoke(arc, args);
        /// <summary>
        /// Invoke the <see cref="ExtractProgress"/> event.
        /// </summary>
        /// <param name="arc">The archive whose progress has changed.</param>
        /// <param name="args">The extraction args.</param>
        protected virtual void EmitOnMoveProgress(DPArchive arc, DPExtractProgressArgs args) => MoveProgress?.Invoke(arc, args);
        /// <summary>
        /// Invoke the <see cref="ArchiveErrored"/> event.
        /// </summary>
        /// <param name="arc">The archive whose progress has changed.</param>
        /// <param name="args">The error args.</param>
        protected virtual void EmitOnArchiveError(DPArchive arc, DPArchiveErrorArgs args) => ArchiveErrored?.Invoke(arc, args);
        /// <summary>
        /// Invoke the <see cref="PeekFinished"/> event.
        /// </summary>
        protected virtual void EmitOnPeekFinished() => PeekFinished?.Invoke();
        /// <summary>
        /// Invoke the <see cref="ExtractFinished"/> event.
        /// </summary>
        protected virtual void EmitOnExtractFinished() => ExtractFinished?.Invoke();
        /// <summary>
        /// Invoke the <see cref="MoveFinished"/> event.
        /// </summary>
        protected virtual void EmitOnMoveFinished() => MoveFinished?.Invoke();

        public DPAbstractExtractor() { }
    }
}
