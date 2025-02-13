using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.DP {
    /// <summary>
    /// A product manager
    /// </summary>
    public interface IDPRecordManager {
        /// <summary>
        /// Creates a new record for an archive after being processed by an <see cref="IDPProcessor"/>.
        /// </summary>
        /// <param name="report">The extraction report for the archive.</param>
        /// <param name="userSettings">The user settings.</param>
        public Task CreateAndAddRecord(DPExtractionReport report, DPSettings userSettings);
    }
}