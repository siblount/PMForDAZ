using Serilog;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// An abstract class for determining the tags of an archive.
    /// </summary>
    public abstract class AbstractTagProvider
    {
        protected virtual ILogger Logger { get; set; } = Log.Logger.ForContext<AbstractTagProvider>();
        /// <summary>
        /// Provides the tags based on <paramref name="arc"/> and it's contents.
        /// </summary>
        /// <param name="arc">The archive to get tags from.</param>
        /// <param name="settings">The settings provided if needed.</param>
        /// <returns>A collection of tags determined for <paramref name="arc"/>.</returns>
        public abstract HashSet<string> GetTags(DPArchive arc, DPProcessSettings settings);

        public AbstractTagProvider() { }
        public AbstractTagProvider(ILogger logger) => Logger = logger;
    }
}
