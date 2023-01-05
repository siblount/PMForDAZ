﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DAZ_Installer.Core
{
    class DPExtractJob
    {
        internal string[] filesToProcess { get; init; }
        internal bool completed { get; set; } = false;
        internal HashSet<string> doNotProcess { get; } = new HashSet<string>();
        internal static DPExtractJob workingJob { get; set; }
        internal static DPTaskManager extractJobs = new DPTaskManager();
        internal static List<DPExtractJob> jobs { get; } = new List<DPExtractJob>(3);
        internal HashSet<string> filesToNotProcess { get; } = new HashSet<string>();
        // TODO: Check if a product is already in list.

        //internal DPExtractJob(string[] files)
        //{
        //    filesToProcess = files;
        //}

        internal DPExtractJob(ListView.ListViewItemCollection files)
        {
            jobs.Add(this);
            List<string> _files = new List<string>(files.Count);

            foreach (ListViewItem file in files)
            {
                _files.Add(file.Text);
            }
            filesToProcess = _files.ToArray();
        }

        internal void DoJob()
        {
            if (workingJob != null && !workingJob.completed && jobs.IndexOf(this) != 0)
            {
                SpinWait.SpinUntil(() => workingJob == null && jobs.IndexOf(this) == 0, -1);
            }
            ParameterizedThreadStart x = new ParameterizedThreadStart(ProcessListAsync);
            var thread = new Thread(x);
            thread.Name = "DP Extract Job";
            thread.Start(filesToProcess);
            if (workingJob != null)
            {
                workingJob.completed = false;
                workingJob = this;
            }
            else
            {
                workingJob = this;
                workingJob.completed = false;
            }
        }

        private void ProcessListAsync(object _arr)
        {
            string[] arr = (string[])_arr;
            var progressCombo = new DPProgressCombo();
            // Snapshot the settings and this will be what we use
            // throughout the entire extraction process.
            var settings = DPSettings.GetCopy();
            for (var i = 0; i < arr.Length; i++)
            {
                if (DPProcessor.doNotProcessList.IndexOf(Path.GetFileName(arr[i])) != -1) continue;

                var x = arr[i];
                progressCombo.ProgressBar.Value = (int)((double)i / arr.Length * 100);
                progressCombo.UpdateText($"Processing archive {i+1}/{arr.Length}: {Path.GetFileName(x)}...({progressCombo.ProgressBar.Value})%");
                DPProcessor.ProcessArchive(x, settings);
            }
            progressCombo.UpdateText($"Finished processing archives");
            progressCombo.ProgressBar.Value = 100;
            var removeFiles = () =>
            {
                foreach (var path in arr)
                {
                    try
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }
                    catch (Exception ex) { DPCommon.WriteToLog($"Failed to delete source: {path}. REASON: {ex}"); }
                }
            };
            switch (settings.permDeleteSource)
            {
                case SettingOptions.Yes:
                    removeFiles();
                    break;
                case SettingOptions.Prompt:
                    var result = MessageBox.Show("Do you wish to PERMENATELY delete all of the source files regardless if it was extracted or not? This cannot be undone.", 
                        "Delete soruce files", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes) removeFiles();
                    break;
            }
            workingJob.completed = true;
            jobs.Remove(this);
            workingJob = null;
            GC.Collect();
            //DoOtherJobs();
        }
    }
}
