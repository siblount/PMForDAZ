﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Database;
using System;
using System.Data;
using System.Windows.Forms;

namespace DAZ_Installer.WinApp.Forms
{
    public partial class DatabaseView : Form
    {
        private DataSet dataset;
        private string lastTableName;

        public DatabaseView()
        {
            InitializeComponent();
            if (DPGlobal.isWindows11) changeTableBtn.Size = new System.Drawing.Size(changeTableBtn.Size.Width, 
                                                                                changeTableBtn.Size.Height + 1);
        }

        public void ShowEverything(DataSet dataSet)
        {
            dataSet?.Dispose();
            dataset = dataSet;
            // This is called away from the UI thread. We have to invoke, otherwise, 
            // external null pointer exception occurs.
            if (InvokeRequired)
            {
                Invoke(new Action(() => dataGrid.DataSource = dataset.Tables[0]));
            }
        }

        private void DatabaseView_Load(object sender, EventArgs e)
        {
            DPDatabase.TableUpdated += OnTableChanged;
            if (DPDatabase.tableNames != null)
            {
                tableNames.Items.AddRange(DPDatabase.tableNames);
                tableNames.SelectedIndex = 0;
            }
        }

        private void changeTableBtn_Click(object sender, EventArgs e)
        {
            if (tableNames.Text.Trim().Length != 0)
                DPDatabase.ViewTableQ(tableNames.Text, 0, ShowEverything);
        }

        private void OnTableChanged(string tableName)
        {
            if (tableName != tableNames.Text) return;
            DPDatabase.ViewTableQ(tableName, callback: ShowEverything);
        }
    }
}
