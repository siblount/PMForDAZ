using DAZ_Installer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using Moq;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.Core.Extraction;
using Serilog;
using System.Text;
using System.Xml;

namespace DAZ_Installer.CoreTests.Extraction
{
    internal static class DPArchiveTestHelpers
    {
        /// <summary>
        /// The default folders and files to add to an archive.
        /// </summary>
        internal static readonly string[] DefaultContents = new[] { "Contents/", "Contents/a.txt", "b.txt", "Contents/A/c.png" };

        /// <summary>
        /// Asserts whether the contents of the archive are as expected.
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        internal static void AssertDefaultContents(IDPArchive arc)
        {
            Assert.AreEqual(3, arc.Contents.Count, "Archive contents count does not match");
            Assert.AreEqual(1, arc.RootFolders.Count, "Archive root folders count does not match");
            Assert.AreEqual(2, arc.Folders.Count, "Archive folders count does not match");
            Assert.AreEqual(1, arc.RootContents.Count, "Archive root contents count does not match");
        }

        /// <summary>
        /// Asserts whether the paths of the entities in the archive are as expected (or whether it was added in successfully).
        /// </summary>
        /// <param name="arc">The archive to check.</param>
        /// <param name="entities">The entities to check.</param>
        public static void AssertExtractorSetPathsCorrectly(IDPArchive arc, IEnumerable<string> entities)
        {
            foreach (var entity in entities)
            {
                if (!arc.Contents.ContainsKey(PathHelper.NormalizePath(entity)) && !arc.Folders.ContainsKey(PathHelper.NormalizePath(entity)))
                    Assert.Fail("Extractor did not correctly set entity paths");
            }
        }

        /// <summary>
        /// Sets up peeking events for the extractor and asserts whether they were raised correctly by peeking into the specified archive.
        /// </summary>
        /// <param name="extractor">The extractor to use.</param>
        /// <param name="arc">The archive for <paramref name="extractor"/> to peek into.</param>
        public static void RunAndAssertPeekEvents(DPAbstractExtractor extractor, IDPArchive arc)
        {
            bool peeked = false;
            extractor.Peeking += () =>
            {
                if (peeked)
                    Assert.Fail("Peeking event was raised more than once");
                peeked = true;
            };
            bool peekFinished = false;
            extractor.PeekFinished += () =>
            {
                if (peekFinished)
                    Assert.Fail("PeekFinished event was raised more than once");
                peekFinished = true;
            };
            extractor.Peek(arc);
            Assert.IsTrue(peeked, "Peeking event was not raised");
            Assert.IsTrue(peekFinished, "PeekFinished event was not raised");
        }

        /// <summary>
        /// Sets up peeking events for the extractor and asserts whether they were raised correctly by peeking into the specified archive.
        /// </summary>
        /// <param name="extractor">The extractor to use.</param>
        /// <param name="arc">The archive for <paramref name="extractor"/> to peek into.</param>
        public static DPExtractionReport RunAndAssertExtractEvents(DPAbstractExtractor extractor, DPExtractSettings settings, bool toTemp = false)
        {
            bool extracting = false;
            extractor.Extracting += () =>
            {
                if (extracting)
                    Assert.Fail("Extracting event was raised more than once");
                extracting = true;
            };
            bool extractFinished = false;
            extractor.ExtractFinished += () =>
            {
                if (extractFinished)
                    Assert.Fail("ExtractFinished event was raised more than once");
                extractFinished = true;
            };
            var report = toTemp ? extractor.ExtractToTemp(settings) : extractor.Extract(settings);
            Assert.IsTrue(extracting, "Extracting event was not raised");
            Assert.IsTrue(extractFinished, "ExtractFinished event was not raised");
            return report;
        }

        public static void AssertExtractFileInfosCorrectlySet(IEnumerable<IDPFile> expectedFilesExtracted)
        {
            foreach (var file in expectedFilesExtracted)
            {
                Assert.IsNotNull(file.FileInfo, $"{file.FileName}'s FileInfo is null, want not null");
                Assert.AreEqual(PathHelper.NormalizePath(file.TargetPath),
                                PathHelper.NormalizePath(file.FileInfo!.Path),
                                $"{file}'s FileInfo's Path does not match TargetPath");
            }
        }

        public static void AssertReport(DPExtractionReport want, DPExtractionReport got)
        {
            CollectionAssert.AreEqual(want.ExtractedFiles, got.ExtractedFiles, "Reports' extracted files are not equal");
            CollectionAssert.AreEqual(want.ErroredFiles.Keys, got.ErroredFiles.Keys, "Reports' errored file keys are not equal");
            Assert.AreEqual(want.Settings, got.Settings, "Reports' settings are not equal");
        }

        /// <summary>
        /// Sets up target paths for the files in the archive to a destination.
        /// </summary>
        /// <remarks>
        /// For each file in the archive, the following is calculated:
        /// <c>file.TargetPath = Path.Combine(basePath, arc.FileName, file.FileName);</c>
        /// </remarks>
        /// <param name="arc">The archive to setup target paths for 
        /// <see cref="DPAbstractExtractor.ExtractToTemp(DPExtractSettings)"/></param>
        /// <param name="basePath">The base path to use for combining the path.</param>
        public static void SetupTargetPaths(IDPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, arc.FileName, file.FileName);
        }

        /// <summary>
        /// Sets up target paths for the files in the archive to a temporary directory.
        /// </summary>
        /// <remarks>
        /// For each file in the archive, the following is calculated:
        /// <c>file.TargetPath = Path.Combine(basePath, file.FileName);</c>
        /// </remarks>
        /// <param name="arc">The archive to setup target paths for 
        /// <see cref="DPAbstractExtractor.ExtractToTemp(DPExtractSettings)"/></param>
        /// <param name="basePath">The base path to use for combining the path.</param>
        /// <seealso cref="SetupTargetPaths(IDPArchive, string)"/>
        public static void SetupTargetPathsForTemp(IDPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, file.FileName);
        }

        /// <summary>
        /// Create a new metadata JSON stream for a DAZ File.
        /// </summary>
        /// <param name="file">The file to create a metadata stream for.</param>
        /// <param name="type">The type to emulate that is written in the stream.</param>
        /// <param name="author">The author to write to the stream.</param>
        /// <param name="email">The email to write to the stream.</param>
        /// <param name="website">The website to write to stream</param>
        /// <returns>A <see cref="MemoryStream"/> of a JSON metadata file.</returns>
        public static Stream CreateMetadataStream(IDPFile file,
                                                  string type = "wearable",
                                                  string author = "TheRealSolly",
                                                  string email = "solomon1blount@gmail.com",
                                                  string website = "www.thesolomonchronicles.com")
        {
            var json = $@"
                {{
                ""file_version"" : ""0.6.0.0"",
                ""asset_info"" : {{
                	""id"" : ""/{file.Path}"",
                	""type"" : ""{type}"",
                	""contributor"" : {{
                		""author"" : ""{author}"",
                		""email"" : ""{email}"",
                		""website"" : ""{website}""
                	}}
                }}
            }}";
            var stream = new MemoryStream(json.Length);
            var sw = new StreamWriter(stream);
            sw.Write(json);
            sw.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Creates a manifest stream based on the files provided.
        /// </summary>
        /// <param name="files">Files to add to the manifest</param>
        /// <returns>A <see cref="MemoryStream"/></returns>
        public static Stream CreateManifestStream(IEnumerable<string> files)
        {
            var stream = new MemoryStream();
            var doc = new XmlDocument();

            var header = doc.CreateElement("DAZInstallManifest");
            header.SetAttribute("VERSION", "0.1");
            doc.AppendChild(header);

            foreach (var file in files)
            {
                var node = doc.CreateElement("File");
                node.SetAttribute("TARGET", "Content");
                node.SetAttribute("ACTION", "Install");
                node.SetAttribute("VALUE", file);

                header.AppendChild(node);
            }
            doc.Save(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Creates a Supplement stream that returns
        /// </summary>
        /// <param name="productName"></param>
        /// <returns>A <see cref="MemoryStream"/></returns>
        public static Stream CreateSupplementStream(string productName = "Gentlemen's Library")
        {
            var supplementStr = "" +
                "<ProductSupplement VERSION=\"0.1\"> " +
                 $"<ProductName VALUE=\"{productName}\"/> " +
                 "<InstallTypes VALUE=\"Content\"/>            " +
                 "<ProductTags VALUE=\"DAZStudio4_5\"/>        " +
                "</ProductSupplement>";

            MemoryStream stream = new(Encoding.ASCII.GetBytes(supplementStr)) { Position = 0 };
            return stream;
        }

        /// <summary>
        /// Returns a file stream based on the <see cref="IDPAbstractNode.FileName"/> or if the extension is in <see cref="DPFile.DAZFormats"/>.
        /// </summary>
        /// <remarks>
        /// The method determines the appropriate stream based on the following conditions:
        /// <list type="bullet">
        /// <item>
        /// <description>If the file is a manifest, it returns a manifest stream from <see cref="CreateManifestStream(IDPArchive, IEnumerable{string})"/></description>
        /// </item>
        /// <item>
        /// <description>If the file is a supplement, it returns a supplement stream from <see cref="CreateSupplementStream(string)"/></description>
        /// </item>
        /// <item>
        /// <description>If the file extension is in <see cref="DPFile.DAZFormats"/>, it returns a metadata stream from <see cref="CreateMetadataStream(IDPFile, string, string, string, string)"/></description>
        /// </item>
        /// <item>
        /// <description>Otherwise, <see cref="Stream.Null"/> is returned</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="file">The file to determine a stream for.</param>
        /// <param name="arc">The archive containing the file.</param>
        /// <param name="pathsForManifest">
        /// Override paths for manifest. If not provided, the method will use the content paths found in <paramref name="arc"/>.
        /// </param>
        /// <returns>A <see cref="Stream"/> object determined by the file type and conditions.</returns>
        public static Stream DetermineFileStream(IDPFile file, IDPArchive arc, IEnumerable<string>? pathsForManifest = null)
        {
            if (file.FileName == "Manifest.dsx") return CreateManifestStream(pathsForManifest ?? arc.Contents.Values.Select(x => x.Path));
            else if (file.FileName == "Supplement.dsx") return CreateSupplementStream();
            else if (DPFile.DAZFormats.Contains(file.Ext)) return CreateMetadataStream(file);
            return Stream.Null;
        }

    }
}
