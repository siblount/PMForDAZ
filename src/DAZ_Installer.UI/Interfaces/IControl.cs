namespace DAZ_Installer.UI
{
    public interface IControl
    {
        /// <inheritdoc cref="Control.BeginInvoke(Action)"/>
        IAsyncResult BeginInvoke(Action action);
        /// <inheritdoc cref="Control.Invoke(Action)"/>
        void Invoke(Action action);
        void ResumeLayout();
        void SuspendLayout();
    }
}
