// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using HtmlAgilityPack;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.DP
{
    internal class DPNetwork
    {
        private static ImageCodecInfo? jpgCodec;
        public ILogger Logger { get; set; } = Log.Logger.ForContext(typeof(DPNetwork));
        // IM[ID]-1_ProductName.zip, where ID = ProductID
        // http://docs.daz3d.com/doku.php/public/read_me/index/[ID]/start
        // Must be filename only.
        /// <summary>
        /// Downloads a thumbnail image for a DAZ product withing the <paramref name="timeout"/> period and 
        /// saves it to the <see cref="DPSettings.ThumbnailsDir"/> directory with 
        /// the <paramref name="fileName"/> + the extension of the image.
        /// </summary>
        /// <param name="fileName">The archive name to download thumbnails from</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal string? DownloadImage(string fileName, TimeSpan timeout)
        {
            var task = Task.Run(() => downloadImage(fileName));
            return task.Wait(timeout) ? task.Result : null;
        }

        private string? downloadImage(string fileName)
        {
            try
            {
                if (jpgCodec is null)
                {
                    Logger.Information("JPEG codec not found, skipping image download.");
                    return null;
                }
                if (fileName.StartsWith("IM"))
                {
                    var ID = int.Parse(fileName[2..fileName.IndexOf('-')]);
                    var link = $@"http://docs.daz3d.com/doku.php/public/read_me/index/{ID}/start";
                    var web = new HtmlWeb();
                    HtmlDocument htmlDoc = web.Load(link);
                    HtmlNode imgNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div[2]/div[2]/div/div/div/p[1]/a/img");
                    if (imgNode == null) return null;
                    var imgLink = imgNode.GetAttributeValue("src", ""); // imgNode is null WHEN PAGE IS NOT FOUND.
                    var equalSignIndex = imgLink.IndexOf("media") + "media=".Length; // +6 = media (5) + equal sign (1)
                    var gcdnLink = WebUtility.UrlDecode(imgLink.Substring(equalSignIndex));
                    if (imgLink != "")
                    {
                        // Download image.
                        using var client = new WebClient();
                        var imgFileName = Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(imgLink);
                        var downloadLocation = Path.Combine(DPSettings.CurrentSettingsObject.ThumbnailsDir, imgFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(downloadLocation));
                        client.DownloadFile(new Uri(gcdnLink), downloadLocation);
                        downscaleImage(downloadLocation);
                        return downloadLocation;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to download image");
            }
            return null;
        }

        /// <summary>
        /// Downscales the image on disk provided by <paramref name="downloadLocation"/> to a 256x256 thumbnail, if possible.
        /// </summary>
        /// <param name="downloadLocation">The location of the image, cannot be null. Does accept invalid paths or paths without access.</param>
        private void downscaleImage(string downloadLocation)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(downloadLocation));

            if (!Directory.Exists(Path.GetDirectoryName(downloadLocation)) || !File.Exists(downloadLocation)) return;

            try
            {
                using var img = Image.FromFile(downloadLocation);
                using var newImg = new Bitmap(256, 256);
                using var graphics = Graphics.FromImage(newImg);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;

                if (img.Size.Width * img.Size.Height > 256 * 256)
                {
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }
                else
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }
                graphics.DrawImage(img, new Rectangle(0, 0, 256, 256));

                var eParams = new EncoderParameters(1);
                eParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                img.Dispose();
                newImg.Save(downloadLocation, jpgCodec, eParams);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to downscale image");
            }
        }

        static DPNetwork()
        {
            try
            {
                var codecs = ImageCodecInfo.GetImageEncoders();
                Log.ForContext<DPNetwork>().Information("Listing all image encoders: {@codecs}", 
                    codecs.Select(x => x.CodecName).ToArray());
                jpgCodec = codecs.FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                if (jpgCodec is null)
                    Log.ForContext<DPNetwork>().Warning("JPEG codec not found!");

            }
            catch (Exception ex)
            {
                Log.ForContext<DPNetwork>().Error(ex, "Failed to get JPEG codec");
            }

        }
    }
}
