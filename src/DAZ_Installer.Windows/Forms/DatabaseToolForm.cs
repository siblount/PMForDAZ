// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Windows.DP;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Forms
{
    public partial class DatabaseToolsForm : Form
    {
        private DataSet? dataset;

        public DatabaseToolsForm()
        {
            InitializeComponent();
            if (DPGlobal.isWindows11) changeTableBtn.Size = new System.Drawing.Size(changeTableBtn.Size.Width,
                                                                                changeTableBtn.Size.Height + 1);
        }

        // This function is called as a callback, so most likely InvokeRequired will be true.
        public void ShowEverything(DataSet? dataSet)
        {
            if (dataSet is null) return;
            if (InvokeRequired)
            {
                BeginInvoke(ShowEverything, dataSet);
                return;
            }
            dataSet?.Dispose();
            dataset = dataSet;
            dataGrid.DataSource = dataset.Tables[0];
        }

        private void DatabaseView_Load(object sender, EventArgs e)
        {
            Program.Database.TableUpdated += OnTableChanged;
            if (Program.Database.TableNames != null)
            {
                tableNames.Items.AddRange(Program.Database.TableNames);
                tableNames.SelectedIndex = 0;
            }
        }

        private void changeTableBtn_Click(object sender, EventArgs e)
        {
            if (tableNames.Text.Trim().Length != 0)
                Program.Database.ViewTableQ(tableNames.Text, 0, ShowEverything);
        }

        private void OnTableChanged(string tableName)
        {
            if (tableName != tableNames.Text) return;
            Program.Database.ViewTableQ(tableName, callback: ShowEverything);
        }

        private void backupDatabaseBtn_Click(object sender, EventArgs e)
        {
            // Check if a backup exists. If it does, ask the user if they want to overwrite it or cancel.
            if (File.Exists(Path.GetFileNameWithoutExtension(Program.Database.Path) + "_backup.db"))
            {
                if (MessageBox.Show("A backup already exists. Do you want to overwrite it?", "Backup Exists", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            if (MessageBox.Show("This may take a while depending on how large the database is. You will be notified when the backup is complete. If you wish to cancel, do it now.",
                    "Prepare for backup", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            Program.Database.BackupDatabaseQ(OnBackupComplete);
        }

        private void OnBackupComplete(bool result)
        {
            if (result)
                MessageBox.Show("Backup database operation has completed", "Backup Complete", MessageBoxButtons.OK);
            else
                MessageBox.Show("Backup database operation has failed, see logs for more info.", "Backup Failed", MessageBoxButtons.OK);
        }

        private void restoreDatabaseBtn_Click(object sender, EventArgs e)
        {
            string location = Path.GetFileNameWithoutExtension(Program.Database.Path) + "_backup.db";
            if (!File.Exists(location))
            {
                if (MessageBox.Show($"A backup has not been detected at {Path.GetFullPath(Program.Database.Path)}. Please locate the backup file to restore on the next prompt.", 
                    "Backup Not Found", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
                var dialog = new OpenFileDialog
                {
                    Filter = "Database Files (*.db)|*.db",
                    Title = "Select the backup database file"
                };
                if (dialog.ShowDialog() == DialogResult.OK) location = dialog.FileName;
                else return;
            }
            if (MessageBox.Show($"Are you sure you want to restore the database?\n\nTHIS WILL OVERWRITE THE CURRENT DATABASE FILE! " +
                                $"\n\nMake a copy of the current database now if you need to, press Yes to proceed or No to cancel.", 
                                "Restore Database", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            Program.Database.RestoreDatabaseQ(location, OnRestoreComplete);
        }

        private void OnRestoreComplete(bool result)
        {
            if (result)
                MessageBox.Show("Restore database operation has completed", "Restore Complete", MessageBoxButtons.OK);
            else
                MessageBox.Show("Restore database operation has failed, see logs for more info.", "Restore Failed", MessageBoxButtons.OK);
        }

        private void vacuumDatabaseBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This may take a while depending on how large the database is. You will be notified when the vacuum is complete. If you wish to cancel, do it now.",
                                   "Prepare for vacuum", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            Program.Database.VacuumDatabaseQ(OnVacuumComplete);
        }

        private void OnVacuumComplete(bool result)
        {
            if (result)
                MessageBox.Show("Vacuum database operation has completed", "Vacuum Complete", MessageBoxButtons.OK);
            else
                MessageBox.Show("Vacuum database operation has failed, see logs for more info.", "Vacuum Failed", MessageBoxButtons.OK);
        }
    }
}
