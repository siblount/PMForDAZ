// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Net;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using DAZ_Installer.Core;
using DAZ_Installer.Core.Utilities;

namespace DAZ_Installer.WinApp
{
    internal static class DPNetwork
    {
        private static ImageCodecInfo jpgCodec;
        // IM[ID]-1_ProductName.zip, where ID = ProductID
        // http://docs.daz3d.com/doku.php/public/read_me/index/[ID]/start
        // Must be filename only.
        internal static string DownloadImage(string fileName)
        {
            try
            {
                if (fileName.StartsWith("IM"))
                {
                    var ID = int.Parse(fileName[2..fileName.IndexOf('-')]);
                    var link = $@"http://docs.daz3d.com/doku.php/public/read_me/index/{ID}/start";
                    var web = new HtmlWeb();
                    var htmlDoc = web.Load(link);
                    var imgNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div[2]/div[2]/div/div/div/p[1]/a/img");
                    if (imgNode == null) return null;
                    var imgLink = imgNode.GetAttributeValue("src", ""); // imgNode is null WHEN PAGE IS NOT FOUND.
                    var equalSignIndex = imgLink.IndexOf("media") + 6; // +6 = media (5) + equal sign (1)
                    var gcdnLink = WebUtility.UrlDecode(imgLink.Substring(equalSignIndex));
                    if (imgLink != "")
                    {
                        // Download image.
                        using (WebClient client = new WebClient())
                        {
                            var imgFileName = Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(imgLink);
                            var downloadLocation = Path.Combine(DPProcessor.settingsToUse.thumbnailsPath, imgFileName);
                            Directory.CreateDirectory(Path.GetDirectoryName(downloadLocation));
                            client.DownloadFile(new Uri(gcdnLink), downloadLocation);
                            Task.Run(() => _downscaleImage(downloadLocation));
                            return downloadLocation;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to download image. REASON: {e}");
            }
            return null;
        }

        /// <summary>
        /// Downscales the image on disk provided by <paramref name="downloadLocation"/> to a 256x256 thumbnail, if possible.
        /// </summary>
        /// <param name="downloadLocation">The location of the image, cannot be null. Does accept invalid paths or paths without access.</param>
        private static void _downscaleImage(string downloadLocation)
        {
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
                graphics.DrawImage(img, new Rectangle(0,0,256,256));

                var eParams = new EncoderParameters(1);
                eParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                img.Dispose();
                newImg.Save(downloadLocation, jpgCodec, eParams);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"DPNetwork was unable to downscale image. REASON: {ex}");
            }
        }

        static DPNetwork()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpgCodec = codec;
                    return;
                }
            }
        }
    }
}
