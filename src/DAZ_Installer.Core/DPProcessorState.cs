namespace DAZ_Installer.Core
{
    public enum ProcessorState
    {
        /// <summary>
        /// The processor is idle and not doing anything.
        /// </summary>
        Idle,
        /// <summary>
        /// The processor is starting up and preparing to process an archive (including nested).
        /// </summary>
        Starting,
        /// <summary>
        /// The processor is determining which files to extract and to where.
        /// </summary>
        PreparingExtraction,
        /// <summary>
        /// The processor has identified files to extract and is extracting them.
        /// </summary>
        Extracting,
        /// <summary>
        /// The processor is currently reading the files to extract.
        /// </summary>
        Peeking,
        /// <summary>
        /// The processor is analyzing the files, fetching tags, reading metadata, etc.
        /// </summary>
        Analyzing,
    }
}
