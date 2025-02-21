using DAZ_Installer.External;
using static DAZ_Installer.External.RAR;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// An interface for the <see cref="RAR"/> class.
    /// </summary>
    public interface IRAR : IDisposable
    {
        /// <inheritdoc cref="RAR.MissingVolume"/>
        event MissingVolumeHandler MissingVolume;
        /// <inheritdoc cref="RAR.NewFile"/>
        event NewFileHandler NewFile;
        /// <inheritdoc cref="RAR.PasswordRequired"/>
        event PasswordRequiredHandler PasswordRequired;
        /// <inheritdoc cref="RAR.ExtractionProgress"/>
        event ExtractionProgressHandler ExtractionProgress;

        /// <inheritdoc cref="RAR.CurrentFile"/>
        /// <remarks>
        /// After a <see cref="ReadHeader"/> call, the function <em>usually</em> sets the current file, but
        /// if it does not throw an exception at the <see cref="ReadHeader"/> call, then the current file will be <see langword="null"/>
        /// because the archive is a multi-volume archive and the file has already been seen in the previous <see cref="ReadHeader"/> call.
        /// </remarks>
        RARFileInfo? CurrentFile { get; }
        /// <inheritdoc cref="RAR.ArchiveData"/>
        RAROpenArchiveDataEx ArchiveData { get; }
        /// <inheritdoc cref="RAR.DestinationPath"/>
        string DestinationPath { get; set; }
        /// <inheritdoc cref="RAR.Close"/>
        void Close();
        /// <inheritdoc cref="RAR.Open(RAR.OpenMode)"/>
        void Open(RAR.OpenMode mode);
        /// <inheritdoc cref="RAR.ReadHeader"/>
        bool ReadHeader();
        /// <inheritdoc cref="RAR.Skip"/>
        void Skip();
        /// <inheritdoc cref="RAR.Test"/>
        void Test();
        /// <inheritdoc cref="RAR.Extract(string)"/>
        void Extract(string destinationName);
    }
}
