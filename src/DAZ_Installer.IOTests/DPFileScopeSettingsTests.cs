using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
    public class DPFileScopeSettingsTests
    {
        public static string tempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.IO.Tests");
        public static string tempPathA = Path.Combine(tempPath, "a.txt");
        public static IEnumerable<string[][]> ConstructorSource => new string[][][]
            {
                //                        Directories                        Files                                     WantDirectories                    WantFiles
                new string[][] { new[] { "Z:\\3D\\My 3D Library" }, new[] { "Z:\\3D\\My 3D Library\\a.txt" }, new[] { "Z:\\3D\\My 3D Library" }, new[] { "Z:\\3D\\My 3D Library\\a.txt" } }, // Test case 1 - Nonexistant on disk.
                new string[][] { new[] { tempPath },                new[] { tempPathA }                     , new[] { tempPath },                new[] { tempPathA }                      }, // Test case 2 - Exists on disk.
                new string[][] { new[] { "Z:\\3D\\My 3D Library" }, Array.Empty<string>()                   , new[] { "Z:\\3D\\My 3D Library" }, Array.Empty<string>()                    }, // Test case 3 - No files listed.
                new string[][] { Array.Empty<string>()            , new[] { "Z:\\3D\\My 3D Library\\a.txt" }, Array.Empty<string>()            , new[] { "Z:\\3D\\My 3D Library\\a.txt" } }, // Test case 4 - No dirs listed.
                new string[][] { new[] { "Z:/3D/My 3D Library" }  , new[] { "Z:/3D/My 3D Library/a.txt" }   , new[] { "Z:\\3D\\My 3D Library" }, new[] { "Z:\\3D\\My 3D Library\\a.txt" } }, // Test case 5 - Different seperator.

            };
        [ClassInitialize]
        public static void SetupClass(TestContext t) => Directory.CreateDirectory(tempPath);

        [ClassCleanup]
        public static void CleanupClass()
        {
            try
            {
                Directory.Delete(tempPath, true);
            }
            catch { Logger.LogMessage("Failed to delete tempPath."); }
        }

        [TestMethod]
        [DynamicData(nameof(ConstructorSource))]
        public void DPFileScopeSettingsTest_IEnumerableConversion(string[] dirs, string[] files, string[] wantDirs, string[] wantFiles)
        {
            var b = new DPFileScopeSettings(files, dirs);
            CollectionAssert.AreEqual(b.WhitelistedDirectories.ToArray(), wantDirs);
            CollectionAssert.AreEqual(b.WhitelistedFilePaths.ToArray(), wantFiles);
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No"), DataRow("D:\\My Private Info\\OMG\\Plz No")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows"), DataRow(".\\Windows")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]


        public void IsDirectoryWhitelistedTest_DenyOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { tempPath });
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No"), DataRow("D:\\My Private Info\\OMG\\Plz No")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows"), DataRow(".\\Windows")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        public void IsDirectoryWhitelistedTest_DenyOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { tempPath }, true, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No"), DataRow("D:\\My Private Info\\OMG\\Plz No")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows"), DataRow(".\\Windows")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        public void IsDirectoryWhitelistedTest_DenyOnStrictFiles(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "C:/a.txt" }, Array.Empty<string>(), false, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/"), DataRow("D:\\")]
        [DataRow("D:/Winners"), DataRow("D:\\Winners")]


        public void IsDirectoryWhitelistedTest_AcceptOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { "D:/", "D:/Winners" });
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/"), DataRow("D:\\")]
        [DataRow("D:/Winners"), DataRow("D:\\Winners")]

        public void IsDirectoryWhitelistedTest_AcceptOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { "D:/", "D:/Winners" }, true, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/"), DataRow("D:\\")]
        [DataRow("D:/Winners"), DataRow("D:\\Winners")]
        [DataRow("C:/Winners"), DataRow("C:\\Winners")]
        [DataRow("../../"), DataRow("..\\..")]


        public void IsDirectoryWhitelistedTest_AcceptOnNoEnforcement(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), true, true, false, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("../Windows"), DataRow("..\\Windows")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        public void IsDirectoryWhitelistedTest_ThrowOnPathTransversal(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), true, true, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.ThrowsException<PathTransversalException>(() => defaultScope.IsDirectoryWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No/b.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows/exploit.exe"), DataRow(".\\Windows\\exploit.exe")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow(".\\exploit.exe"), DataRow("./exploit.exe"), DataRow(".\\exploit.exe")]

        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        public void IsFilePathWhitelistedTest_DenyOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/My Private Info/OMG/Plz No/a.txt", "C:/a.txt" }, new string[] { "D:/" });
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No/a.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("D:/My Private Info/OMG/Plz No/b.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows/exploit.exe"), DataRow(".\\Windows\\exploit.exe")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow(".\\exploit.exe"), DataRow("./exploit.exe"), DataRow(".\\exploit.exe")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("C:/a.txt"), DataRow("C:\\a.txt")]
        public void IsFilePathWhitelistedTest_DenyOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/My Private Info/OMG/Plz No/a.txt", "C:/a.txt" }, new string[] { "D:/" }, true, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }
        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No/b.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows/exploit.exe"), DataRow(".\\Windows\\exploit.exe")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow(".\\exploit.exe"), DataRow("./exploit.exe"), DataRow(".\\exploit.exe")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        [DataRow("C:/b.txt"), DataRow("C:\\b.txt")]
        [DataRow("C:/lavarball.txt"), DataRow("C:\\lavarball.txt")]
        public void IsFilePathWhitelistedTest_DenyOnStrictFiles(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/My Private Info/OMG/Plz No/a.txt", "C:/a.txt" }, new string[] { "D:/" }, false, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("C:/My Private Info/OMG/Plz No/a.txt"), DataRow("C:\\My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("b.txt")]
        [DataRow("C:/a.txt"), DataRow("C:\\a.txt")]
        public void IsFilePathWhitelistedTest_DenyOnNoExplicit(string path)
        {
            var defaultScope = new DPFileScopeSettings(new[] { "a.txt", "D:/a.txt" }, new[] { "D:/" }, false);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No/a.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("D:/My Private Info/OMG/Plz No/b.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        public void IsFilePathWhitelistedTest_AcceptOnNoEnforcement(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), noEnforcement: true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }


        [DataTestMethod]
        [DataRow("D:/My Private Info/OMG/Plz No/a.txt"), DataRow("D:\\My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("a.txt")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        public void IsFilePathWhitelistedTest_AcceptOnNoExplicit(string path)
        {
            var defaultScope = new DPFileScopeSettings(new[] { "a.txt", "D:/a.txt" }, new[] { "D:/" }, false);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        [DataRow("D:/c"), DataRow("D:\\c")]
        public void IsFilePathWhitelistedTest_AcceptOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { "D:/" });
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        public void IsFilePathWhitelistedTest_AcceptOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/a.txt", "D:/b.txt" }, new string[] { "D:/" }, true, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [DataTestMethod]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        [DataRow("C:/c.txt"), DataRow("C:\\c.txt")]

        public void IsFilePathWhitelistedTest_AcceptOnStrictFiles(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/a.txt", "D:/b.txt", "C:/c.txt" }, new string[] { "D:/" }, false, true);
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }
        [DataTestMethod]
        [DataRow("D:/a.txt"), DataRow("D:\\a.txt")]
        [DataRow("D:/b.txt"), DataRow("D:\\b.txt")]
        [DataRow("C:/c.txt"), DataRow("C:\\c.txt")]

        public void IsFilePathWhitelistedTest_AcceptsFilesOutOfDirViaDefinedFiles(string path)
        {
            // this test checks to see if we whitelist files that are outside of the defined directories, but are explicitly whitelisted in the files.
            var defaultScope = new DPFileScopeSettings(new string[] { "D:/a.txt", "D:/b.txt", "C:/c.txt" }, new string[] { "D:/" });
            if (path.StartsWith('.')) path = Path.Join(tempPath, path); // see if we can get out of the temp dir.
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }
        [TestMethod]
        public void CreateUltraStrictTest()
        {
            var scope = DPFileScopeSettings.CreateUltraStrict(new string[] { "D:/a.txt" }, new string[] { "D:/" });
            Assert.IsFalse(scope.NoEnforcement);
            Assert.IsTrue(scope.ThrowOnPathTransversal);
            Assert.IsTrue(scope.ExplicitDirectoryPaths);
            Assert.IsTrue(scope.ExplicitFilePaths);
        }
        [TestMethod]
        public void NoEnforcementTest()
        {
            DPFileScopeSettings scope = DPFileScopeSettings.All;
            Assert.IsTrue(scope.NoEnforcement);
            Assert.IsFalse(scope.ThrowOnPathTransversal);
            Assert.IsFalse(scope.ExplicitDirectoryPaths);
            Assert.IsFalse(scope.ExplicitFilePaths);
        }
    }
}