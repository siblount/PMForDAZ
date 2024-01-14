using System.Diagnostics;

namespace DAZ_Installer.Core.Extraction.Fakes
{
    /// <summary>
    /// A fake process that implements <see cref="IProcess"/> and all implemented properties and methods are virtual.
    /// </summary>
    internal class FakeProcess : IProcess
    {
        public virtual StreamWriter StandardInput => StreamWriter.Null;
        public virtual ProcessStartInfo StartInfo { get; set; } = new();
        public virtual bool HasExited { get; set; }
        public virtual bool EnableRaisingEvents { get; set; } = false;

        public virtual event Action<string?>? OutputDataReceived;
        public virtual event Action<string?>? ErrorDataReceived;
        public virtual event Action? Exited;

        public virtual IEnumerable<string?> OutputEnumerable { get; set; } = Enumerable.Empty<string>();
        public virtual IEnumerable<string?> ErrorEnumerable { get; set; } = Enumerable.Empty<string>();
        public virtual IEnumerator<string?>? OutputEnumerator { get; set; } = null;
        public virtual IEnumerator<string?>? ErrorEnumerator { get; set; } = null;
        /// <summary>
        /// Begins raising the <see cref="ErrorDataReceived"/> event.
        /// It will emit error data to the <see cref="ErrorDataReceived"/> event via <see cref="ErrorEnumerable"/>. <para/>
        /// Additionally, if any <see langword="null"/> values are provided, it will stop until the next call to <see cref="BeginErrorReadLine"/>. <para/>
        /// This method is virtual.
        /// </summary>
        public virtual void BeginErrorReadLine()
        {
            if (!EnableRaisingEvents) throw new Exception("EnableRaisingEvents must be true to use this method.");
            ErrorEnumerator ??= ErrorEnumerable.GetEnumerator();
            while (ErrorEnumerator.MoveNext())
            {
                var line = ErrorEnumerator.Current;
                ErrorDataReceived?.Invoke(line);
                if (line is null) return;
            }
            ErrorEnumerator = null;
            ErrorDataReceived?.Invoke(null);
            HasExited = true;
        }
        /// <summary>
        /// Begins raising the <see cref="OutputDataReceived"/> event.
        /// It will emit error data to the <see cref="OutputDataReceived"/> event via <see cref="OutputEnumerable"/>. <para/>
        /// Additionally, if any <see langword="null"/> values are provided, it will stop until the next call to <see cref="BeginErrorReadLine"/>. <para/>
        /// 
        /// This method is virtual.
        /// </summary>
        public virtual void BeginOutputReadLine()
        {
            if (!EnableRaisingEvents) throw new Exception("EnableRaisingEvents must be true to use this method.");
            OutputEnumerator ??= OutputEnumerable.GetEnumerator();
            while (OutputEnumerator.MoveNext())
            {
                var line = OutputEnumerator.Current;
                OutputDataReceived?.Invoke(line);
                if (line is null) return;
            }
            OutputEnumerator = null;
            OutputDataReceived?.Invoke(null);
        }
        /// <summary>
        /// Does nothing.
        /// </summary>
        public virtual void Dispose() { }
        /// <summary>
        /// Sets <see cref="HasExited"/> to true.
        /// </summary>
        /// <param name="entireProcessTree">does nothing</param>
        public virtual void Kill(bool entireProcessTree) => HasExited = true;
        /// <summary>
        /// Does nothing.
        /// </summary>
        public virtual void Start() { }
        public virtual bool WaitForExit(int milliseconds) => SpinWait.SpinUntil(() => HasExited, milliseconds);
        /// <summary>
        /// Returns an array of strings that represent 7z output. At [0] = path, [1] = size, [2] = attributes, [3] = separator.
        /// </summary>
        /// <param name="entity">The path to get 7z info from.</param>
        /// <returns>The lines of output for the entity.</returns>
        public static void GetLinesForEntity(string entity, in List<string> listToAddStringsTo)
        {
            var fname = Path.GetFileName(entity);
            listToAddStringsTo.AddRange(new[]
            {
                "----------",
                "Path = " + entity,
                "Size = " + 1,
                "Attributes = " + (string.IsNullOrEmpty(fname) ? "D" : ""),
            });
        }
    }
}
