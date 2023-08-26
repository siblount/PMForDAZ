using DAZ_Installer.Core.Extraction;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Core;
using Serilog;
using System.Text.Json;
using System.Xml;

namespace DAZ_Installer.Core.Tests
{
    internal class DPProcessorTestHelpers
    {
        /// <summary>
        /// Contains the DefaultContents from <see cref="DPArchiveTestHelpers"/> and the manifest and supplement."/>
        /// </summary>
        public static IEnumerable<string> DefaultContents => new string[] { "Manifest.dsx", "Supplement.dsx", "Contents/a.txt", "Contents/b.txt", "Contents/Documents/c.png", "Contents/Documents/d.txt", "Contents/e.duf", "Contents/f.duf", "bullshit.png" };
        public static MockOptions DefaultMockOptions => new();
        public static AssertOptions DefaultAssertOptions => new();
        public struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialRAR = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public IEnumerable<string> paths = DefaultContents;
            public Func<DPExtractionReport>? ExtractToTempFunc = null;
            public Func<DPExtractionReport>? ExtractFunc = null;

            public MockOptions() { }
        }
        public static DPArchive NewMockedArchive(MockOptions options, out DPAbstractExtractor extractor, out FakeDPFileInfo fakeDPFileInfo, out FakeFileInfo fakeFileInfo)
        {
            fakeFileInfo = options.partialFileInfo ? Substitute.ForPartsOf<FakeFileInfo>("Z:/test.rar", null) : Substitute.For<FakeFileInfo>("Z:/test.rar", null);
            fakeDPFileInfo = options.partialDPFileInfo ? Substitute.ForPartsOf<FakeDPFileInfo>(fakeFileInfo, new FakeDPIOContext(), null) : Substitute.For<FakeDPFileInfo>(fakeFileInfo, Substitute.ForPartsOf<FakeDPIOContext>(), null);
            extractor = Substitute.For<DPAbstractExtractor>();
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo, extractor);
            extractor.ExtractToTemp(default).ReturnsForAnyArgs(x =>
            {
                if (options.ExtractToTempFunc is null) return handleExtract(x);
                return options.ExtractToTempFunc();
            });
            extractor.Extract(default).ReturnsForAnyArgs(x => {
                if (options.ExtractFunc is null) return handleExtract(x);
                return options.ExtractFunc();
            });
            return arc;
        }

        public static DPProcessor SetupProcessor(DPArchive arc, IContextFactory? factory)
        {
            var ctxFactory = factory ?? new MockedFakeDPIOContextFactory();
            var p = new DPProcessor()
            {
                Logger = Log.Logger.ForContext<DPProcessor>(),
                ContextFactory = ctxFactory
            };
            SetupEntities(DefaultContents, arc);
            UpdateFileInfos(new DPExtractSettings("A:/", arc.Contents.Values, archive: arc), ctxFactory);
            return p;
        }

        private static DPExtractionReport handleExtract(CallInfo x)
        {
            UpdateFileInfos(x.Arg<DPExtractSettings>(), new MockedFakeDPIOContextFactory());
            return new DPExtractionReport()
            {
                ErroredFiles = new(0),
                ExtractedFiles = x.Arg<DPExtractSettings>().FilesToExtract.ToList(),
                Settings = x.Arg<DPExtractSettings>()
            };
        }

        private static void SetupEntities(IEnumerable<string> paths, DPArchive arc)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(Path.GetFileName(path))) new DPFolder(path, arc, null);
                else DPFile.CreateNewFile(path, arc, null);
            }
        }

        private static void UpdateFileInfos(DPExtractSettings settings, IContextFactory factory)
        {
            var ctx = factory.CreateContext(settings.Archive.Context);
            foreach (var file in settings.Archive.Contents.Values)
            {
                var path = string.IsNullOrEmpty(file.TargetPath) ? Path.Combine(settings.TempPath, file.Path) : file.TargetPath;
                file.FileInfo = ctx.CreateFileInfo(path);
                file.FileInfo.WhenForAnyArgs(x => x.Open(default, default)).DoNotCallBase();
                file.FileInfo.WhenForAnyArgs(x => x.TryAndFixOpenRead(out var _, out _)).DoNotCallBase();
                file.FileInfo.TryAndFixOpenRead(out var _, out _).ReturnsForAnyArgs(x =>
                {
                    x[0] = file.FileInfo.Open(default, default);
                    x[1] = null;
                    return true;
                });
                if (path.EndsWith("Manifest.dsx")) file.FileInfo.Open(default, default).ReturnsForAnyArgs(_ => createManifestStream(settings.Archive));
                else if (path.EndsWith("Supplement.dsx")) file.FileInfo.Open(default, default).ReturnsForAnyArgs(_ => createSupplementStream());
                else if (DPFile.DAZFormats.Contains(file.Ext)) file.FileInfo.Open(default, default).ReturnsForAnyArgs(_ => createMetadataStream(file));
            }
        }

        private static void SetupSupportFiles(DPArchive arc)
        {
            foreach (var file in arc.Contents.Values)
            {
                if (file.FileName.Contains("Manifest.dsx"))
                    file.FileInfo.Open(default, default).ReturnsForAnyArgs(_ => createManifestStream(arc));
                else if (file.FileName.Contains("Supplement.dsx"))
                    file.FileInfo.Open(default, default).ReturnsForAnyArgs(_ => createSupplementStream());
            }
        }

        private static Stream createMetadataStream(DPFile file)
        {
            var stream = new MemoryStream();
            var json = @"
                {
                ""file_version"" : ""0.6.0.0"",
                ""asset_info"" : {
                	""id"" : ""/{arcPath}"",
                	""type"" : ""wearable"",
                	""contributor"" : {
                		""author"" : ""TheRealSolly"",
                		""email"" : ""solomon1blount@gmail.com"",
                		""website"" : ""www.thesolomonchronicles.com""
                	},
                }


            ";
            var sw = new StreamWriter(stream);
            sw.Write(json.Replace("{arcPath}", file.Path));
            sw.Flush();
            stream.Position = 0;
            return stream;

        }

        private static Stream createManifestStream(DPArchive arc)
        {
            var stream = new MemoryStream();
            var doc = new XmlDocument();

            var header = doc.CreateElement("DAZInstallManifest");
            header.SetAttribute("VERSION", "0.1");
            doc.AppendChild(header);

            foreach (var file in arc.Contents.Values)
            {
                var node = doc.CreateElement("File");
                node.SetAttribute("TARGET", "Content");
                node.SetAttribute("ACTION", "Install");
                node.SetAttribute("VALUE", file.Path);

                header.AppendChild(node);
            }
            doc.Save(stream);
            stream.Position = 0;
            return stream;
        }

        private static Stream createSupplementStream()
        {
            MemoryStream stream = new();
            XmlDocument doc = new();
            // Write the header:
            doc.LoadXml("" +
                "<ProductSupplement VERSION=\"0.1\">" +
                 "<ProductName VALUE=\"Gentlemen's Library\"/> " +
                 "<InstallTypes VALUE=\"Content\"/>            " +
                 "<ProductTags VALUE=\"DAZStudio4_5\"/>        " +
                "</ProductSupplement>"
            );
            doc.Save(stream);
            stream.Position = 0;
            return stream;
        }

        public struct AssertOptions
        {
            public int ExpectedProcessErrorCount = 0;
            public int ExpectedArchiveCount = 1;
            public int ExpectedFileErrorCount = 0;
            public Dictionary<string, DPExtractionReport>? ExpectArchiveProcessed = null;

            public AssertOptions() { }
        }

        public static void AttachCommonEventHandlers(DPProcessor processor, AssertOptions opts)
        {
            int arcEnterCount = 0, arcExitCount = 0;
            int processErrorCount = 0;
            processor.ArchiveEnter += (_, e) =>
            {
                if (++arcEnterCount > opts.ExpectedArchiveCount) Assert.Fail("Archive Enter called more than expected");
            };
            processor.ArchiveExit += (_, e) =>
            {
                arcExitCount++;
                if (opts.ExpectArchiveProcessed is null) return;
                if (!opts.ExpectArchiveProcessed.TryGetValue(e.Archive.FileName, out var wantReport)) return;
                AssertReport(wantReport, e.Report);
            };
            processor.ProcessError += (_, e) =>
            {
                if (++processErrorCount > opts.ExpectedProcessErrorCount) Assert.Fail("Process Error called more than expected");
            };
            processor.Finished += () =>
            {
                if (arcEnterCount != arcExitCount) Assert.Fail("Archive Enter and Exit counts do not match");
            };
        }

        public static DPExtractionReport CreateExtractionReport(DPExtractSettings settings, IEnumerable<string> failedFiles, IEnumerable<string>? successFiles)
        {
            var fh = failedFiles.ToHashSet();
            var sh = successFiles?.ToHashSet() ?? new HashSet<string>();
            var extractedFiles = successFiles?.ToList() ?? settings.Archive.Contents.Values.Where(m => !fh.Contains(m.Path)).Select(x => x.Path);
            if (fh.Intersect(sh).Count() > 1) Assert.Inconclusive("Failed and Success files intersect");
            return new DPExtractionReport()
            {
                Settings = settings,
                ErroredFiles = failedFiles.ToDictionary(m => CreateDummyFile(m), _ => string.Empty),
                ExtractedFiles = successFiles?.Select(x => new DPFile(x, null, null, null, null!)).ToList() ?? new List<DPFile>()
            };
        }

        public static DPFile CreateDummyFile(string path) => new(path, null, null, null, null!);
        public static DPFolder CreateDummyFolder(string path) => new(path, null, null!);

        public static void AssertReport(DPExtractionReport want, DPExtractionReport got)
        {
            var a = want.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            var b = got.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Errored file paths are not equal");
            a = want.ExtractedFiles.Select(x => x.Path).ToArray();
            b = got.ExtractedFiles.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Extracted file paths are not equal");

            // Settings
            Assert.AreSame(want.Settings.Archive, got.Settings.Archive, "Reports' archive are not the same");
            CollectionAssert.AreEqual(want.Settings.FilesToExtract.Select(x => x.Path).ToArray(), got.Settings.FilesToExtract.Select(x => x.Path).ToArray(), "Reports' files to extract are not the same");
            Assert.AreEqual(want.Settings.OverwriteFiles, got.Settings.OverwriteFiles, "Reports' overwrite files are not the same");
        }

        public static List<string> CalculateExpectedFiles(IEnumerable<string> files) => files.Where(x => !string.IsNullOrEmpty(Path.GetFileName(x))).ToList();

        public static DPExtractSettings CreateExtractSettings(IEnumerable<string> paths, DPArchive arc) => new("A:/", paths.Where(x => !string.IsNullOrEmpty(Path.GetFileName(x))).Select(x => CreateDummyFile(x)), archive: arc);

    }
}
