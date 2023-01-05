// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.WinApp.Forms
{
    public partial class PasswordInput : Form
    {
        protected internal string password;
        public string archiveName = string.Empty;
        public string message = string.Empty;
        public PasswordInput()
        {
            InitializeComponent();
        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            password = maskedTextBox1.Text;
            Close();
        }

        private void PasswordInput_Load(object sender, EventArgs e)
        {
            if (message.Length == 0)
            {
                mainLbl.Text = $"{archiveName} is password-protected. Please enter password to decrypt file.";
            } else
            {
                mainLbl.Text = message;
            }
        }

        private void PasswordInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            password = maskedTextBox1.Text;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
