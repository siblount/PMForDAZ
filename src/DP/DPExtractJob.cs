// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DAZ_Installer
{
    class DPExtractJob
    {
        internal string[] filesToProcess { get; init; }
        internal bool completed { get; set; } = false;
        internal HashSet<string> doNotProcess { get; } = new HashSet<string>();
        internal static DPExtractJob workingJob { get; set; }
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
            thread.Start(filesToProcess);
            if (workingJob != null)
            {
                workingJob.completed = false;
                workingJob = this;
            } else
            {
                workingJob = this;
                workingJob.completed = false;
            }
        }

        private void ProcessListAsync(object _arr)
        {
            string[] arr = (string[]) _arr;
            var progressCombo = (Control[])extractControl.extractPage.Invoke(new Func<Control[]>(extractControl.extractPage.createProgressCombo));
            
            for (var i = 0; i < arr.Length; i++)
            {
                if (DPProcessor.doNotProcessList.IndexOf(Path.GetFileName(arr[i])) != -1) continue;

                var x = arr[i];
                ((ProgressBar)progressCombo[2]).Value = (int)(((double)i / arr.Length) * 100);
                progressCombo[1].Text = $"Processing archive {i}/{arr.Length}: {Path.GetFileName(x)}...({i / arr.Length})%";
                extractControl.extractPage.mainProcLbl.Text = progressCombo[1].Text;
                DPProcessor.ProcessArchive(x);
            }
            progressCombo[1].Text = $"Finished processing archives";
            ((ProgressBar)progressCombo[2]).Value = 100;
            extractControl.extractPage.mainProcLbl.Text = progressCombo[1].Text;
            GC.Collect();
            workingJob.completed = true;
            jobs.Remove(this);
            workingJob = null;
            //DoOtherJobs();
        }
    }
}
