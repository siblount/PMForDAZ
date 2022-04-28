// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAZ_Installer.DP;

namespace DAZ_Installer
{
    public partial class ProductRecordForm : Form
    {
        public ProductRecordForm()
        {
            InitializeComponent();
        }

        public ProductRecordForm(DPProductRecord productRecord)
        {
            InitializeComponent();
            SetupView(productRecord);
        }

        public void SetupView(DPProductRecord record)
        {
            productNameTxtBox.Text = record.ProductName;
            tagsTxtBox.Text = string.Join(", ", record.Tags);
            if (record.ThumbnailPath != null && File.Exists(record.ThumbnailPath))
            {
                thumbnailBox.Image = Library.self.AddReferenceImage(record.ThumbnailPath);
            }
        }
    }
}
