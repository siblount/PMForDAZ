namespace DAZ_Installer.UI
{
    /// <summary>
    /// Provides a way to show message boxes.
    /// </summary>
    public interface IMessageBoxProvider
    {
        /// <inheritdoc cref="MessageBox.Show(string?, string?, MessageBoxButtons, MessageBoxIcon)"/>
        DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
        /// <inheritdoc cref="MessageBox.Show(string?, string?, MessageBoxButtons)"/>
        DialogResult Show(string text, string caption, MessageBoxButtons buttons);
        /// <inheritdoc cref="MessageBox.Show(string?, string?)"/>
        DialogResult Show(string text, string caption);
    }
}
