namespace DAZ_Installer.Core
{
    /// <summary>
    /// A factory for creating <see cref="DPFile"/>s.
    /// </summary>
    public class DPFileFactory : IDPFileFactory
    {
        /// <summary>
        /// A singleton instance of the <see cref="DPFileFactory"/> class.
        /// </summary>
        /// <returns>A singleton instance.</returns>
        public static DPFileFactory Instance { get; } = new DPFileFactory();

        /// <summary>
        /// A constructor to prevent instantiation of the class.
        /// </summary>
        private DPFileFactory() { }
        /// <summary>
        /// A factory method that creates a new file based on the extension of the file. 
        /// If the extension is not recognized, then a regular <see cref="DPFile"/> is created.
        /// If the extension is recognized, then a specialized file is created. <br/>
        /// If the extension is "dsf" or "duf", then a <see cref="DPDazFile"/> is created. <br/>
        /// If the extension is "dsx", then a <see cref="DPDSXFile"/> is created. <br/>
        /// If the extension is in <see cref="AcceptableImportFormats"/>, then a <see cref="DPArchive"/> is created. <br/>
        /// </summary>
        /// <param name="path">The path to set for this file.</param>
        /// <param name="arc">The associated archive to set for this file, if any.</param>
        /// <param name="parent">The parent folder for this file, if any.</param>
        /// <returns>Either a <see cref="DPArchive"/>, <see cref="DPDazFile"/>, <see cref="DPDSXFile"/>, or a <see cref="DPFile"/>.</returns>
        public IDPFile CreateNewFile(string path, IDPArchive? arc, IDPFolder? parent)
        {
            var ext = DPFile.GetExtension(path);
            if (ext == "dsf" || ext == "duf")
            {
                ArgumentNullException.ThrowIfNull(arc, nameof(arc));
                return new DPDazFile(path, arc, parent);
            }
            else if (ext == "dsx")
            {
                ArgumentNullException.ThrowIfNull(arc, nameof(arc));
                return new DPDSXFile(path, arc, parent);
            }
            else if (DPFile.AcceptableImportFormats.Contains(ext))
                return new DPArchive(path, arc, parent);
            return new DPFile(path, arc, parent);
        }
    }
}