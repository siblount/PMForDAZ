using System.Windows.Forms;

namespace DAZ_Installer.Windows.Forms
{
    public partial class AboutForm : Form
    {
        public static string AboutString =
            "Copyright © Solomon Blount" + "\n" +
            $"{Program.AppName} {Program.AppVersion} {Program.VersionSuffix}" + "\n" +
            "\n" +
            $"{Program.AppName} is an application that allows users to install and manage their products for DAZ Studio from any vendor supporting common packaging formats. ";

        public AboutForm()
        {
            InitializeComponent();
            mainInfoLbl.Text = AboutString;
            titleLbl.Text = Program.AppName;
        }
    }
}
