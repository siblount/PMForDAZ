using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
    public class DPFileScopeSettingsTests
    {
        public static readonly string tempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.IO.Tests");
        public static readonly string tempPathA = Path.Combine(tempPath, "a.txt");
        public static readonly string nonExistantRootPath = GetRandomPathRoot();
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
        
        private static string GetPath(string path)
        {
            if (path[0] == '~') return Path.Join(Directory.GetDirectoryRoot(tempPath), path[1..]);
            else if (path[0] == '!') return Path.Join(nonExistantRootPath, path[1..]);
            else if (path.Length >= 2 && path[0] == '.' && path[1] != '.') return Path.Join(tempPath, path);
            else return path;
        }

        /// <summary>
        /// Returns a random path root that is not the same as the current directory (even if it does not exist).
        /// </summary>
        /// <returns>A random drive letter with the colon and slash after it.</returns>
        public static string GetRandomPathRoot()
        {
            var currentDrive = Path.GetPathRoot(Directory.GetCurrentDirectory())![0];
            var availableDrives = Enumerable.Range('A', 'Z' - 'A' + 1)
                                    .Select(c => (char)c)
                                    .Where(c => c != currentDrive).ToList();
            return availableDrives[new Random().Next(availableDrives.Count)].ToString() + ":/";
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No"), DataRow("~My Private Info\\OMG\\Plz No")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("~.."), DataRow("~../"), DataRow("~..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows"), DataRow(".\\Windows")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]


        public void IsDirectoryWhitelistedTest_DenyOnDefault(string path)
        {
            Logger.LogMessage($"FileInfo Interpret: {new FileInfo(GetPath(path)).FullName}");
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { tempPath });
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No"), DataRow("~My Private Info\\OMG\\Plz No")]
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
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No"), DataRow("~My Private Info\\OMG\\Plz No")]
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
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("!a.txt") }, Array.Empty<string>(), false, true);
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~"), DataRow("~")]
        [DataRow("~Winners"), DataRow("~Winners")]


        public void IsDirectoryWhitelistedTest_AcceptOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { GetPath("~"), GetPath("~Winners") });
            path = GetPath(path); 
            Logger.LogMessage($"Path: {path}");
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~"), DataRow("~")]
        [DataRow("~Winners"), DataRow("~Winners")]

        public void IsDirectoryWhitelistedTest_AcceptOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { GetPath("~"), GetPath("~Winners") }, true, true);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~"), DataRow("~")]
        [DataRow("~Winners"), DataRow("~Winners")]
        [DataRow("!Winners"), DataRow("!Winners")]
        [DataRow("../../"), DataRow("..\\..")]


        public void IsDirectoryWhitelistedTest_AcceptOnNoEnforcement(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), true, true, false, true);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("~.."), DataRow("~../"), DataRow("~..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("../Windows"), DataRow("..\\Windows")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        public void IsDirectoryWhitelistedTest_ThrowOnPathTransversal(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), true, true, true);
            path = GetPath(path); 
            Assert.ThrowsException<PathTransversalException>(() => defaultScope.IsDirectoryWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No/b.txt"), DataRow("~My Private Info\\OMG\\Plz No\\b.txt")]
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
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~My Private Info/OMG/Plz No/a.txt"), GetPath("!a.txt" )}, new string[] { GetPath("~") });
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No/a.txt"), DataRow("~My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("~My Private Info/OMG/Plz No/b.txt"), DataRow("~My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows/exploit.exe"), DataRow(".\\Windows\\exploit.exe")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow(".\\exploit.exe"), DataRow("./exploit.exe"), DataRow(".\\exploit.exe")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("!a.txt"), DataRow("!a.txt")]
        public void IsFilePathWhitelistedTest_DenyOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~My Private Info/OMG/Plz No/a.txt"), GetPath("!a.txt" )}, new string[] { GetPath("~") }, true, true);
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }
        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No/b.txt"), DataRow("~My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        [DataRow("./Windows/exploit.exe"), DataRow(".\\Windows\\exploit.exe")]
        [DataRow(".\\"), DataRow("./"), DataRow(".\\")]
        [DataRow(".\\exploit.exe"), DataRow("./exploit.exe"), DataRow(".\\exploit.exe")]
        [DataRow("top secret/../../../Windows"), DataRow("top secret\\..\\..\\..\\Windows")]
        [DataRow("top secret.jpg\\..\\..\\..\\Windows"), DataRow("top secret.jpg//..//..//..\\Windows")]
        [DataRow("%2e%2e%2f"), DataRow("%2e%2e%5c")]
        [DataRow(".\\%2e%2e%2f"), DataRow("../%2e%2e%2f")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        [DataRow("!b.txt"), DataRow("!b.txt")]
        [DataRow("!lavarball.txt"), DataRow("!lavarball.txt")]
        public void IsFilePathWhitelistedTest_DenyOnStrictFiles(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~My Private Info/OMG/Plz No/a.txt"), GetPath("!a.txt" )}, new string[] { GetPath("~") }, false, true);
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("!My Private Info/OMG/Plz No/a.txt"), DataRow("!My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("!b.txt")]
        [DataRow("!a.txt"), DataRow("!a.txt")]
        public void IsFilePathWhitelistedTest_DenyOnNoExplicit(string path)
        {
            var defaultScope = new DPFileScopeSettings(new[] { GetPath("a.txt"), GetPath("~a.txt") }, new[] { GetPath("~") }, false);
            path = GetPath(path); 
            Assert.IsFalse(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No/a.txt"), DataRow("~My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("~My Private Info/OMG/Plz No/b.txt"), DataRow("~My Private Info\\OMG\\Plz No\\b.txt")]
        [DataRow(".."), DataRow("../"), DataRow("..\\")]
        [DataRow("../../Windows"), DataRow("..\\..\\Windows")]
        public void IsFilePathWhitelistedTest_AcceptOnNoEnforcement(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), Array.Empty<string>(), noEnforcement: true);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }


        [TestMethod]
        [DataRow("~My Private Info/OMG/Plz No/a.txt"), DataRow("~My Private Info\\OMG\\Plz No\\a.txt")]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("a.txt")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        public void IsFilePathWhitelistedTest_AcceptOnNoExplicit(string path)
        {
            var defaultScope = new DPFileScopeSettings(new[] { GetPath("a.txt"), GetPath("~a.txt") }, new[] { GetPath("~") }, false);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        [DataRow("~c"), DataRow("~c")]
        public void IsFilePathWhitelistedTest_AcceptOnDefault(string path)
        {
            var defaultScope = new DPFileScopeSettings(Array.Empty<string>(), new string[] { GetPath("~") });
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        public void IsFilePathWhitelistedTest_AcceptOnStrict(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~a.txt"), GetPath("~b.txt") }, new string[] { GetPath("~") }, true, true);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }

        [TestMethod]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        [DataRow("!c.txt"), DataRow("!c.txt")]

        public void IsFilePathWhitelistedTest_AcceptOnStrictFiles(string path)
        {
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~a.txt"), GetPath("~b.txt"), GetPath("!c.txt") }, new string[] { GetPath("~") }, false, true);
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }
        [TestMethod]
        [DataRow("~a.txt"), DataRow("~a.txt")]
        [DataRow("~b.txt"), DataRow("~b.txt")]
        [DataRow("!c.txt"), DataRow("!c.txt")]

        public void IsFilePathWhitelistedTest_AcceptsFilesOutOfDirViaDefinedFiles(string path)
        {
            // this test checks to see if we whitelist files that are outside of the defined directories, but are explicitly whitelisted in the files.
            var defaultScope = new DPFileScopeSettings(new string[] { GetPath("~a.txt"), GetPath("~b.txt"), GetPath("!c.txt") }, new string[] { GetPath("~") });
            path = GetPath(path); 
            Assert.IsTrue(defaultScope.IsFilePathWhitelisted(path));
        }
        [TestMethod]
        public void CreateUltraStrictTest()
        {
            var scope = DPFileScopeSettings.CreateUltraStrict(new string[] { GetPath("~a.txt") }, new string[] { GetPath("~") });
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