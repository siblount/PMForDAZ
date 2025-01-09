namespace DAZ_Installer.UI
{
    public interface IProgressCombo : IControl
    {
        /// <summary>
        /// The associated <see cref="CancellationToken"/> for the progress bar.
        /// </summary>
        public CancellationToken Token { get; }
        /// <summary>
        /// Starts the progress bar. Automatically checks if Invoke is required.
        /// </summary>
        /// <remarks>
        /// <see cref="CancellationTokenSource"/> is reset here. This should be paired with
        /// an <see cref="EndProgress"/> call to ensure the progress bar is properly closed.
        /// </remarks>
        /// <seealso cref="EndProgress"/>
        void StartProgress();
        /// <summary>
        /// Ends the progress bar. Automatically checks if Invoke is required. 
        /// </summary>
        /// <remarks>
        /// This should be called first by a <see cref="StartProgress"/> call.
        /// </remarks>
        /// <seealso cref="StartProgress"/>
        void EndProgress();
        /// <summary>
        /// Sets the progress of the progress bar. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="value">The value to set</param>
        void SetProgress(int value);
        /// <summary>
        /// Changes the process bar style to either <see cref="ProgressBarStyle.Marquee"/> or <see cref="ProgressBarStyle.Blocks"/>". 
        /// It automatically checks if Invoke is required.
        /// </summary>
        /// <param name="marqueue">Whether to set the progress bar style to Marqueue or not.</param>
        public void ChangeProgressBarStyle(bool marqueue);
        /// <summary>
        /// Sets the text of the progress bar label and the main process label. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="text">The text to set it to.</param>
        public void SetText(string text);
    }
}
