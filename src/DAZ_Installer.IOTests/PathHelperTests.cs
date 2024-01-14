using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
    public class PathHelperTests
    {
        [DataTestMethod]
        [DataRow("", "", "")]
        [DataRow("Contents\\Documents\\sollybean", "Contents", "Contents/Documents/sollybean")]
        [DataRow("C:/Contents/Documents/sollybean", "C:/Contents", "Contents/Documents/sollybean")]
        [DataRow("C:/Contents/Documents/sollybean.txt", "C:/Contents", "Contents/Documents/sollybean.txt")]
        [DataRow("Content/Documents/Roblox", "Content", "Content/Documents/Roblox")]
        [DataRow("My Library/data/DubNation/Golden State Warriors/suck.dsf", "My Library/data", "data/DubNation/Golden State Warriors/suck.dsf")]
        [DataRow("C:/My Library/data/2015/Cavaliers/winners.dsf", "C:/My Library", "My Library/data/2015/Cavaliers/winners.dsf")]
        [DataRow("Content\\data\\john.dsf", "", "Content\\data\\john.dsf")]
        [DataRow("Content\\data\\john.dsf", "Content/data", "data/john.dsf")]



        public void GetRelativePathOfRelativeParentTest(string path, string relativeTo, string want) => Assert.AreEqual(want, PathHelper.GetRelativePathOfRelativeParent(path, relativeTo));

        [DataTestMethod]
        [DataRow("C:\\", '\\')]
        [DataRow("", '/')]
        [DataRow("Contents/Documents/sollybean", '/')]
        [DataRow("Contents/Documents\\sollybean", '/')]

        public void GetSeperatorTest(string path, char want) => Assert.AreEqual(want, PathHelper.GetSeperator(path));

        [DataTestMethod]
        [DataRow("C:/", "")]
        [DataRow("C:/Contents/Documents/sollybean", "Documents")]
        [DataRow("C:/Contents/Documents/sollybean/", "Documents")]
        [DataRow("C:\\John\\a.png", "John")]
        [DataRow("a.png", "")]
        [DataRow("My Library/a.png", "My Library")]
        public void GetLastDirTest(string path, string want)
        {
            var file = path.EndsWith(".png");
            Assert.AreEqual(want, PathHelper.GetLastDir(path, file));
        }

        [DataTestMethod]
        [DataRow("Contents", "")]
        [DataRow("C:/Contents/Documents/sollybean", "C:/Contents/Documents")]
        [DataRow("Contents/Documents/sollybean", "Contents/Documents")]
        [DataRow("Contents/Documents", "Contents")]

        public void GetParentTest(string path, string want) => Assert.AreEqual(want, PathHelper.GetParent(path));

        [DataTestMethod]
        [DataRow("C:/Contents/Documents/sollybean", "sollybean")]
        [DataRow("C:/Contents/Documents/sollybean/", "")]
        [DataRow("C:/Contents/Documents/sollybean.", "sollybean.")]
        [DataRow("C:/Contents/Documents/sollybean.txt", "sollybean.txt")]
        [DataRow("sollybean.txt", "sollybean.txt")]
        [DataRow("sollybean", "sollybean")]
        [DataRow("sollybean/", "")]
        [DataRow("C:\\", "")]
        [DataRow("C:\\Dog/food/a.jpg", "a.jpg")]
        [DataRow("", "")]
        public void GetFileNameTest(string path, string want) => Assert.AreEqual(want, PathHelper.GetFileName(path));

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("C:/Contents/Documents/sollybean", "C:/Contents/Documents/sollybean")]
        [DataRow("C:/Contents/Documents/sollybean/", "C:/Contents/Documents/sollybean")]
        [DataRow("Contents/Documents/sollybean", "Contents/Documents/sollybean")]
        [DataRow("Contents/Documents/sollybean/", "Contents/Documents/sollybean")]
        [DataRow("Documents", "Documents")]
        [DataRow("Documents/", "Documents")]

        public void CleanDirPathTest(string path, string want) => Assert.AreEqual(want, PathHelper.CleanDirPath(path));

        [DataTestMethod]
        [DataRow("", "", 0)]
        [DataRow("C:/Contents/Documents/sollybean", "C:/Contents/Documents/sollybean", 0)]
        [DataRow("Documents", "Documents", 0)]
        [DataRow("Content/Documents/Roblox", "Content", 2)]
        public void GetNumOfLevelsAboveTest(string path, string relativeTo, int want) => Assert.AreEqual(want, PathHelper.GetNumOfLevelsAbove(path, relativeTo));

        [DataTestMethod]
        [DataRow("", 0)]
        [DataRow("C:/Contents/Documents/sollybean", 3)]
        [DataRow("Documents", 0)]
        [DataRow("Documents/", 0)]
        [DataRow("Content/Documents/Roblox", 2)]
        [DataRow("Content/Documents/Roblox", 2)]
        public void GetSubfoldersCountTest(string path, int want) => Assert.AreEqual(want, PathHelper.GetSubfoldersCount(path));

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("Documents", "Documents")]
        [DataRow("Documents\\", "Documents/")]
        [DataRow("Documents\\dom.txt", "Documents/dom.txt")]
        [DataRow("C:\\Documents\\dom.txt", "C:/Documents/dom.txt")]
        [DataRow("C:\\Documents/dom.txt", "C:\\Documents\\dom.txt")]
        [DataRow("C:/Documents/dom.txt", "C:\\Documents\\dom.txt")]
        public void SwitchSeperatorsTest(string path, string want) => Assert.AreEqual(want, PathHelper.SwitchSeperators(path));

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("Documents", "Documents")]
        [DataRow("Documents/text/", "Documents/text")]
        [DataRow("Documents/text/a.png", "Documents/text/a.png")]
        public void GetDirectoryPathTest(string path, string want) => Assert.AreEqual(want, PathHelper.GetDirectoryPath(path));

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("Documents", "Documents")]
        [DataRow("Documents\\", "Documents")]
        [DataRow("Documents\\dom.txt", "Documents/dom.txt")]
        [DataRow("C:\\Documents\\dom.txt", "C:/Documents/dom.txt")]
        [DataRow("C:\\Documents/dom.txt", "C:/Documents/dom.txt")]
        [DataRow("/\\/\\/\\", "")]
        public void NormalizePathTest(string path, string want) => Assert.AreEqual(want, PathHelper.NormalizePath(path));

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("Documents", "")]
        [DataRow("Documents/Roblox", "Documents")]
        [DataRow("Documents/Roblox\\People", "Documents/Roblox")]
        [DataRow("data\\DAZ 3D", "data")]
        [DataRow("data\\DAZ 3D\\Genesis 8", "data\\DAZ 3D")]
        [DataRow("data/DAZ 3D/Genesis 8/Male/Morphs/Lexa Kiness/Tidazo/LK Tidazo Body.dsf", "data/DAZ 3D/Genesis 8/Male/Morphs/Lexa Kiness/Tidazo")]

        [DataRow("My Library/data/TheRealSolly/The Little League's Court", "My Library/data/TheRealSolly")]

        public void UpTest(string path, string want) => Assert.AreEqual(want, PathHelper.Up(path));

        [DataTestMethod]
        [DataRow("", false)]
        [DataRow("a.png", false)]
        [DataRow("invalid_but_not_transversal..", false)]
        [DataRow(".............................", false)]
        [DataRow("C:/a.png", false), DataRow("C:\\a.png", false)]
        [DataRow("C:\\Windows\\a.valid..name", false), DataRow("C:/Windows/a.valid..name", false)]
        [DataRow("C:\\Windows\\a.valid_-'..name", false), DataRow("C:/Windows/a.valid_-..name", false)]
        [DataRow("C:\\Windows\\a.valid_-'..-_name", false), DataRow("C:/Windows/a.valid_-_..-_name", false)]
        [DataRow("C:\\_invalid_but_not_transversal..", false), DataRow("C:/a_invalid_but_not_transversal..", false)]
        [DataRow("..", true)]
        [DataRow(".", true)]
        [DataRow("..\\", true), DataRow("../", true)]
        [DataRow("C:\\..\\a.valid..name", true), DataRow("C:/../a.valid..name", true)]
        [DataRow("C:\\.\\a.valid_-'..name", true), DataRow("C:/./a.valid_-..name", true)]
        [DataRow("abc\\..", true), DataRow("abc/..", true)]
        [DataRow("abc\\.", true), DataRow("abc/.", true)]


        public void CheckForTranversalTest(string path, bool want) => Assert.AreEqual(want, PathHelper.CheckForTranversal(path));
    }
}