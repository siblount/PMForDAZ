using System.Windows.Forms;

namespace DAZ_Installer.UI
{
    /// <summary>
    /// Uses the default <see cref="MessageBox"/> to provide message boxes.
    /// </summary>
    public class MessageBoxProvider : IMessageBoxProvider
    {
        /// <summary>
        /// The singleton instance of the <see cref="MessageBoxProvider"/>.
        /// </summary>
        public static readonly MessageBoxProvider Instance = new();
        /// <inheritdoc/>
        public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
        /// <inheritdoc/>
        public DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(text, caption, buttons);
        }
        /// <inheritdoc/>
        public DialogResult Show(string text, string caption)
        {
            return MessageBox.Show(text, caption);
        }
    }
}
