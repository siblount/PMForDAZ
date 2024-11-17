namespace DAZ_Installer.Core
{
    /// <summary>
    /// A factory for creating new <see cref="IDPFile"/> objects.
    /// </summary>
    public interface IDPFileFactory
    {
        /// <summary>
        /// Creates a new file and assigns it to the specified archive and parent folder, if any.
        /// </summary>
        /// <param name="path">The raw path to set for this new file.</param>
        /// <param name="arc">The asosciated archive for this file, if any.</param>
        /// <param name="parent">The parent for this file, if any.</param>
        /// <returns>A new <see cref="IDPFile"/> object.</returns>
        public IDPFile CreateNewFile(string path, IDPArchive? arc, IDPFolder? parent);
    }
}