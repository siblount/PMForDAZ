using System.Diagnostics;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// An implementation of <see cref="IProcess"/> that wraps the <see cref="Process"/> class.
    /// </summary>
    internal class ProcessWrapper : IProcess
    {
        private readonly Process process = new();
        public event Action<string?>? OutputDataReceived 
        { 
            add => process.OutputDataReceived += (_, e) => value?.Invoke(e.Data); 
            remove => process.OutputDataReceived -= (_, e) => value?.Invoke(e.Data);
        }
        public event Action<string?>? ErrorDataReceived 
        { 
            add => process.ErrorDataReceived += (_, e) => value?.Invoke(e.Data); 
            remove => process.ErrorDataReceived -= (_, e) => value?.Invoke(e.Data); 
        }
        public event Action Exited
        {
            add => process.Exited += (_, __) => value?.Invoke(); 
            remove => process.Exited -= (_, __) => value?.Invoke(); 
        }
        public StreamWriter StandardInput => process.StandardInput;
        public bool HasExited => process.HasExited;
        public bool EnableRaisingEvents { get => process.EnableRaisingEvents; set => process.EnableRaisingEvents = value; }
        public ProcessStartInfo StartInfo { get => process.StartInfo; set => process.StartInfo = value; }

        internal ProcessWrapper() { }

        public void BeginErrorReadLine() => process.BeginErrorReadLine();
        public void BeginOutputReadLine() => process.BeginOutputReadLine();
        public void Kill(bool entireProcessTree) => process.Kill(entireProcessTree);
        public void Start() => process.Start();
        public bool WaitForExit(int milliseconds) => process.WaitForExit(milliseconds);

        public void Dispose()
        {
            process.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
