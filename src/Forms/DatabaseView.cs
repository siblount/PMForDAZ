// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace DAZ_Installer.Forms
{
    public partial class DatabaseView : Form
    {
        private DataSet dataset;
        private string lastTableName;

        public DatabaseView()
        {
            InitializeComponent();
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
            if (DP.DPDatabase.tableNames != null)
            {
                tableNames.Items.AddRange(DP.DPDatabase.tableNames);
                tableNames.SelectedIndex = 0;
            }
        }

        private void changeTableBtn_Click(object sender, EventArgs e)
        {
            if (tableNames.Text.Trim().Length != 0)
                DP.DPDatabase.ViewTableQ(tableNames.Text, 0, ShowEverything);
        }
    }
}
