using DAZ_Installer.Windows.DP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.Models.Components
{
    internal class DPExtractQueueManager : IDPExtractQueueManager
    {
        public static readonly DPExtractQueueManager Instance = new();

        public void AddArchiveToQueue(string filePath, string groupKey)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void UpdateArchiveStatus(string filePath, DPArchiveStatus status, IEnumerable<DPArchiveInfo.ErrorInfo> errors)
        {
            throw new NotImplementedException();
        }
    }
}
