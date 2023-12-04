using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DAZ_Installer.Core.Integration
{
    internal class DPIntegrationArchiveHelpers
    {
        const string ManifestContent = 
        @"<DAZInstallManifest VERSION=""0.1"">
            <GlobalID VALUE=""this doesnt matter ayy lmao""/>
            <File TARGET=""Content"" ACTION=""Install"" VALUE=""Content\data\TheRealSolly\data.dsf""/>
            <File TARGET=""Content"" ACTION=""Install"" VALUE=""Content\data\TheRealSolly\a.txt""/>
            <File TARGET=""Content"" ACTION=""Install"" VALUE=""Content\data\TheRealSolly\b.txt""/>
        </DAZInstallManifest>";

        const string SupplementContent = 
        @"<ProductSupplement VERSION=""0.1"">
            <ProductName VALUE=""Test Product""/>
            <InstallTypes VALUE=""Content""/>
            <ProductTags VALUE=""DAZStudio4_5""/>
        </ProductSupplement>";

        const string DSFContent =
        @"{
            ""file_version"" : ""0.6.0.0"",
            ""asset_info"" : {
                ""id"" : ""/data/data.dsf"",
                ""type"" : ""prop"",
                ""contributor"" : {
                    ""author"" : ""TheRealSolly"",
                    ""email"" : ""solomon1blount@gmail.com"",
                    ""website"" : ""www.thesolomonchronicles.com""
                },
                ""revision"" : ""1.0"",
                ""modified"" : ""2020-12-06T00:04:11Z""
            }
        }";
        /// <summary>
        /// Creates a dummy file at the <paramref name="path"/> with random <paramref name="n"/> bytes.
        /// </summary>
        /// <param name="path">The path to save the dummy file to.</param>
        /// <param name="n">The number of random bytes to save to file.</param>
        /// <exception cref="IOException"></exception>"
        public static void CreateDummyFile(string path, uint n)
        {
            //uint n = 10 * (uint)Math.Pow(2, 20); // 10 MB.
            uint l = n / 65536;
            var bytes = new byte[n / l];
            using var file = File.Create(path);
            for (var i = 0; i < l; i++)
            {
                new Random().NextBytes(bytes);
                file.Write(bytes, 0, bytes.Length);
            }
        }

        public static void CreateManifestFile(string path)
        {
            using var file = File.CreateText(path);
            file.Write(ManifestContent);
            file.Close();
        }

        public static void CreateSupplementFile(string path)
        {
            using var file = File.CreateText(path);
            file.Write(SupplementContent);
            file.Close();
        }

        public static void CreateDSFFile(string path)
        {
            using var file = File.CreateText(path);
            file.Write(DSFContent);
            file.Close();
        }

        public static List<string> CreateArchiveContents(string basePath)
        {
            // File structure
            // basePath
            // - data
            //  - TheRealSolly
            //      - data.dsf
            //      - a.txt
            // - docs
            //  - b.txt
            // - not included in content folders
            //   - this should not be processed.txt
            //   - random_image_for_splitting.jpg 

            var dataPath = Path.Combine(basePath, "data");
            var docsPath = Path.Combine(basePath, "docs");
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(dataPath);
            Directory.CreateDirectory(docsPath);
            var notIncludedPath = Path.Combine(basePath, "not included in content folders");
            Directory.CreateDirectory(notIncludedPath);
            var snbpFile = Path.Combine(notIncludedPath, "this should not be processed.txt");
            File.Create(snbpFile).Close();
            var imgFile = Path.Combine(notIncludedPath, "random_image_for_splitting.jpg");
            CreateDummyFile(imgFile, 10 * (uint)Math.Pow(2, 20));
            var bFile = Path.Combine(docsPath, "b.txt");
            File.Create(bFile).Close();
            var theRealSollyPath = Path.Combine(dataPath, "TheRealSolly");
            Directory.CreateDirectory(theRealSollyPath);
            var aFile = Path.Combine(theRealSollyPath, "a.txt");
            File.Create(aFile).Close();
            var dataFile = Path.Combine(theRealSollyPath, "data.dsf");
            CreateDSFFile(dataFile);
            return new List<string>() { snbpFile, bFile, aFile, dataFile, imgFile };
        }

        public static void CreateDAZArchiveContents(string basePath)
        {
            // File structure
            // basePath
            // - manifest.dsx
            // - supplement.dsx
            // - random_image_for_splitting.jpg
            // - Content
            //   - data
            //     - TheRealSolly
            //       - data.dsf
            //       - a.txt
            // - docs
            //   - b.txt
            // - not included in content folders
            //   - this should not be processed.txt

            var contentPath = Path.Combine(basePath, "Content");
            var dataPath = Path.Combine(contentPath, "data");
            var docsPath = Path.Combine(contentPath, "docs");
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(dataPath);
            Directory.CreateDirectory(docsPath);
            var notIncludedPath = Path.Combine(basePath, "not included in content folders");
            Directory.CreateDirectory(notIncludedPath);
            File.Create(Path.Combine(notIncludedPath, "this should not be processed.txt")).Close();
            File.Create(Path.Combine(docsPath, "b.txt")).Close();
            var theRealSollyPath = Path.Combine(dataPath, "TheRealSolly");
            Directory.CreateDirectory(theRealSollyPath);
            File.Create(Path.Combine(theRealSollyPath, "a.txt")).Close();
            CreateManifestFile(Path.Combine(basePath, "manifest.dsx"));
            CreateSupplementFile(Path.Combine(basePath, "supplement.dsx"));
            CreateDSFFile(Path.Combine(theRealSollyPath, "data.dsf"));
        }
        public static void AssertDefaultContentsNonDAZ(DPArchive arc)
        {
            Assert.AreEqual(5, arc.Contents.Count, "Archive contents count does not match");
            Assert.AreEqual(3, arc.RootFolders.Count, "Archive root folders count does not match");
            Assert.AreEqual(4, arc.Folders.Count, "Archive folders count does not match");
            Assert.AreEqual(0, arc.RootContents.Count, "Archive root contents count does not match");
        }
        public static void AssertDefaultContentsDAZ(DPArchive arc)
        {
            Assert.AreEqual(7, arc.Contents.Count, "Archive contents count does not match");
            Assert.AreEqual(3, arc.RootFolders.Count, "Archive root folders count does not match");
            Assert.AreEqual(5, arc.Folders.Count, "Archive folders count does not match");
            Assert.AreEqual(3, arc.RootContents.Count, "Archive root contents count does not match");
        }

    }
}
