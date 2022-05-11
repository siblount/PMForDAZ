namespace DAZ_Installer.DP {
    internal class DPZipArchive : DPAbstractArchive
    {

        internal DPZipArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase) {
            
        }

        internal override void Extract()
        {
            throw new System.NotImplementedException();
        }

        internal override void Peek()
        {
            throw new System.NotImplementedException();
        }

        public void HandleProgressionZIP(ref ZipArchive sender, int i, int max)
        {
            var percentComplete = (float)i / max;
            Control[] progressCombo;
            Label progressLabel;
            ProgressBar progressBar;
            var index = ArrayHelper.GetIndex(progressStack, sender);

            // If already exists, update controls.
            if (index != -1)
            {
                progressCombo = controlComboStack[index];
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];
            }
            else
            {
                DPCommon.WriteToLog("Creating new progression combo.");
                progressCombo = (Control[])Invoke(new Func<Control[]>(createProgressCombo));
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];

                var openSlotIndex = ArrayHelper.GetNextOpenSlot(progressStack);
                if (openSlotIndex == -1)
                {
                    throw new IndexOutOfRangeException("Attempted to add more than 3 to array.");
                }
                else
                {
                    progressStack[openSlotIndex] = sender;
                    controlComboStack[openSlotIndex] = progressCombo;
                }
                DPProcessor.workingArchive.progressCombo = progressCombo;
            }
            var progress = (int)Math.Floor(percentComplete * 100);

            progressLabel.Text = $"Extracting files...({progress}%)";
            mainProcLbl.Text = progressLabel.Text;
            progressBar.Value = progress;
        }

    }
}