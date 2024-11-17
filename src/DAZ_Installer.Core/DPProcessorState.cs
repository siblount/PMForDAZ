namespace DAZ_Installer.Core
{
    /// <summary>
    /// The state of the processor.
    /// </summary>
    public enum ProcessorState : byte
    {
        /// <summary>
        /// The processor is idle and not doing anything.
        /// </summary>
        Idle = 0,
        /// <summary>
        /// The processor is starting up and preparing to process an archive (including nested).
        /// </summary>
        Starting = 1,
        /// <summary>
        /// The processor is determining which files to extract and to where.
        /// </summary>
        PreparingExtraction = 2,
        /// <summary>
        /// The processor has identified files to extract and is extracting them.
        /// </summary>
        Extracting = 4,
        /// <summary>
        /// The processor is currently reading the files to extract.
        /// </summary>
        Peeking = 8,
        /// <summary>
        /// The processor is analyzing the files, fetching tags, reading metadata, etc.
        /// </summary>
        Analyzing = 16,
    }
}
