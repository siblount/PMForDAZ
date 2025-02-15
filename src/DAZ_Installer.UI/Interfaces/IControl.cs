namespace DAZ_Installer.UI
{
    public interface IControl
    {
        bool InvokeRequired { get; }
        /// <inheritdoc cref="Control.BeginInvoke(Action)"/>
        IAsyncResult BeginInvoke(Action action);
        /// <inheritdoc cref="Control.Invoke(Action)"/>
        void Invoke(Action action);
        void ResumeLayout();
        void SuspendLayout();
    }
}
