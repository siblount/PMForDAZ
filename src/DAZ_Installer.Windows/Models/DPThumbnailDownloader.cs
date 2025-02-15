// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
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

namespace DAZ_Installer.Windows.DP
{
    /// <inheritdoc/>
    public sealed class DPThumbnailDownloader : IDPThumbnailDownloader
    {
        /// <summary>
        /// Singleton object used for accessing this object.
        /// </summary>
        public static readonly DPThumbnailDownloader Instance = new();
        private static ImageCodecInfo? jpgCodec;
        /// <summary>
        /// The logger for this class to use.
        /// </summary>
        public ILogger Logger { get; set; } = Log.ForContext<DPThumbnailDownloader>();
        /// <summary>
        /// The message handler to use for sending requests.
        /// </summary>
        /// <returns>By default, a <see cref="HttpClientHandler"/> with mostly default settings.</returns>
        public HttpMessageHandler MessageHandler { get; set; } = new HttpClientHandler() { MaxAutomaticRedirections = 5 };
        private DPThumbnailDownloader() {}
        // IM[ID]-1_ProductName.zip, where ID = ProductID
        // http://docs.daz3d.com/doku.php/public/read_me/index/[ID]/start
        // Must be filename only.
        /// <inheritdoc/>
        /// <remarks>Downloads from docs.daz3d.com/doku.php/public/read_me/index/`ID`/start</remarks>
        public async Task<IDPFileInfo?> DownloadThumbnail(string ID, string imageName, IDPDirectoryInfo saveDirectoryInfo)
        {
            try
            {
                if (jpgCodec is null)
                {
                    Logger.Information("JPEG codec not found, skipping image download.");
                    return null;
                }

                if (!int.TryParse(ID, out var _)) {
                    Logger.Error("Not a valid ID");
                    return null;
                }
                
                var link = $@"http://docs.daz3d.com/doku.php/public/read_me/index/{ID}/start";
                var web = new HtmlWeb();
                HtmlDocument htmlDoc = await web.LoadFromWebAsync(link);
                HtmlNode imgNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div[2]/div[2]/div/div/div/p[1]/a/img");
                // imgNode is null WHEN PAGE IS NOT FOUND.
                if (imgNode is null) return null;
                var imgLink = imgNode.GetAttributeValue("src", ""); 
                var equalSignIndex = imgLink.IndexOf("media") + "media=".Length;
                var gcdnLink = WebUtility.UrlDecode(imgLink.Substring(equalSignIndex));
                if (string.IsNullOrEmpty(imgLink)) return null;
                
                var imgFileName = Path.GetFileNameWithoutExtension(imageName) + Path.GetExtension(imgLink);
                saveDirectoryInfo.TryCreate();
                var fileInfo = saveDirectoryInfo.FileSystem.CreateFileInfo(imgFileName);
                if (!await DownloadImageAsync(gcdnLink, fileInfo)) return null;
                return fileInfo;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to download image");
            }
            return null;
        }

        private async Task<bool> DownloadImageAsync(string imageUrl, IDPFileInfo fileInfo)
        {
            try
            {
                using var client = new HttpClient(MessageHandler);
                using var response = await client.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Warning("Failed to download image. Status code: {StatusCode}", response.StatusCode);
                    Logger.Debug("Response: {@response}", response);
                    return false;
                }

                // Verify content type is an image
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Warning("Downloaded content is not an image. Content-Type: {ContentType}", contentType);
                    return false;
                }

                // Download and process the image
                using var imageStream = await response.Content.ReadAsStreamAsync();
                
                // Verify it's a valid image before processing
                try
                {
                    imageStream.Position = 0;
                    using var img = Image.FromStream(imageStream);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Downloaded file is not a valid image: {file}", fileInfo.Name);
                    return false;
                }

                // Downscale the image
                imageStream.Position = 0;
                using var downscaledStream = DownscaleImage(imageStream);
                if (downscaledStream == null)
                {
                    Logger.Error("Failed to downscale image: {file}", fileInfo.Name);
                    return false;
                }

                // Save the downscaled image
                if (!fileInfo.TryAndFixOpenWrite(out var fileStream, out var fileEx))
                {
                    Logger.Error(fileEx, "Failed to open a write stream to file {file}", fileInfo.Name);
                    return false;
                }

                using (fileStream!)
                {
                    await downscaledStream.CopyToAsync(fileStream!);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error downloading image from {Url}", imageUrl);
                return false;
            }
        }

        /// <summary>
        /// Downscales the image.
        /// </summary>
        /// <param name="imageStream">The image stream to update.</param>
        private MemoryStream? DownscaleImage(Stream imageStream)
        {
            if (jpgCodec is null)
            {
                Logger.Error("JPEG codec not found, skipping downscale.");
                return null;
            }

            try
            {
                using var originalImage = Image.FromStream(imageStream);
                using var newImg = new Bitmap(256, 256);
                using var graphics = Graphics.FromImage(newImg);
                
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;

                // Choose quality settings based on image size
                if (originalImage.Size.Width * originalImage.Size.Height > 256 * 256)
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

                graphics.DrawImage(originalImage, new Rectangle(0, 0, 256, 256));

                // Prepare encoder parameters
                using var eParams = new EncoderParameters(1);
                eParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                // Save to a new memory stream
                var resultStream = new MemoryStream();
                newImg.Save(resultStream, jpgCodec, eParams);
                resultStream.Position = 0; // Reset position for reading
                return resultStream;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to downscale image");
                return null;
            }
        }

        static DPThumbnailDownloader()
        {
            try
            {
                _ = Pens.Black; // In .NET 8 & above, someone from Microsoft forgot to initialize
                                // the static fields of the System.Drawing namespace when GetImageEncoders() is called first.
                                // Without this, ImageCodecInfo will raise an AccessViolationException and crash.
                                // This is the workaround until fixed.
                                // https://github.com/dotnet/winforms/issues/12494
                var codecs = ImageCodecInfo.GetImageEncoders();
                Log.ForContext<DPThumbnailDownloader>().Debug("Listing all image encoders: {@codecs}", 
                    codecs.Select(x => x.CodecName).ToArray());
                jpgCodec = codecs.FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                if (jpgCodec is null)
                    Log.ForContext<DPThumbnailDownloader>().Warning("JPEG codec not found!");

            }
            catch (Exception ex)
            {
                Log.ForContext<DPThumbnailDownloader>().Error(ex, "Failed to get JPEG codec");
            }

        }
    }
}
