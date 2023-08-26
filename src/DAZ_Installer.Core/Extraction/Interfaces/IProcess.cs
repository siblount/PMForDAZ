using System.Diagnostics;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// Interface for the <see cref="Process"/> class.
    /// </summary>
    internal interface IProcess : IDisposable
    {
        /// <inheritdoc cref="Process.StandardInput"/>
        StreamWriter StandardInput { get; }
        /// <inheritdoc cref="Process.StartInfo"/>
        ProcessStartInfo StartInfo { get; set; }
        /// <summary>
        /// Occurs each time an application writes a line to its redirected <see cref="Process.StandardOutput"/> stream. <br/>
        /// /// Returns the line written. Returns <see langword="null"/> if stream ended.
        /// </summary>
        event Action<string?>? OutputDataReceived;
        /// <summary>
        /// Occurs each time an application writes a line to its redirected <see cref="Process.StandardError"/> stream. <br/>
        /// Returns the line written. Returns <see langword="null"/> if stream ended.
        /// </summary>
        event Action<string?>? ErrorDataReceived;
        /// <inheritdoc cref="Process.Exited"/>
        event Action Exited;
        /// <inheritdoc cref="Process.HasExited"/>
        bool HasExited { get; }
        /// <inheritdoc cref="Process.EnableRaisingEvents"/>
        bool EnableRaisingEvents { get; set; }
        /// <inheritdoc cref="Process.Start()"/>
        void Start();
        /// <inheritdoc cref="Process.Kill(bool)"/>
        void Kill(bool entireProcessTree);
        /// <inheritdoc cref="Process.BeginOutputReadLine"/>
        void BeginOutputReadLine();
        /// <inheritdoc cref="Process.BeginErrorReadLine"/>
        void BeginErrorReadLine();
        /// <inheritdoc cref="Process.WaitForExit(int)"/>
        bool WaitForExit(int milliseconds);

    }
}
