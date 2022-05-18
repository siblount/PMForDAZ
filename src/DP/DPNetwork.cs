// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace DAZ_Installer.DP
{
    internal class DPNetwork
    {
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
                            var downloadLocation = Path.Combine(DPSettings.thumbnailsPath, imgFileName);
                            Directory.CreateDirectory(Path.GetDirectoryName(downloadLocation));
                            client.DownloadFile(new Uri(gcdnLink), downloadLocation);
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
    }
}
