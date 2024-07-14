using Microsoft.VisualStudio.TestTools.UnitTesting;
using Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build.Tests
{
    [TestClass]
    public class VersionVersionUpdaterTests
    {
        [TestMethod]
        [DataRow("1.0.0", "Pre-alpha", "1.0.0\nPre-alpha")]
        [DataRow("1.0.0", "Alpha", "1.0.0\nAlpha")]
        public void UpdateVersionTest(string version, string suffix, string expected)
        {
            var result = VersionVersionUpdater.UpdateVersion(version, suffix);
            Assert.AreEqual(expected, result);
        }
    }
}