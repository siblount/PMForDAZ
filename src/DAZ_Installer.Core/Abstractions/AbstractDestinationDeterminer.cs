using Serilog;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// An abstract class for determining the destination of files inside of an archive.
    /// </summary>
    public abstract class AbstractDestinationDeterminer
    {
        virtual protected ILogger Logger { get; set; } = Log.Logger.ForContext<AbstractDestinationDeterminer>();
        /// <summary>
        /// Determines the files to extract inside of the <paramref name="arc"/> and sets their <see cref="DPAbstractNode.TargetPath"/> to their destination based on the <paramref name="settings"/>.
        /// </summary>
        /// <param name="arc">The archive to determine files to extract and determine destinations.</param>
        /// <param name="settings">The settings to base decisions off of.</param>
        /// <returns>A collection of <see cref="DPFile"/>s determined to be processed.</returns>
        /// <exception cref="Exception"></exception>
        public abstract HashSet<DPFile> DetermineDestinations(DPArchive arc, DPProcessSettings settings);

        public AbstractDestinationDeterminer() { }

        public AbstractDestinationDeterminer(ILogger logger) => Logger = logger;
    }
}
