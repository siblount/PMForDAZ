using Build;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Build.Tests
{
    [TestClass]
    public class ISSVersionUpdaterTests
    {
        private const string SampleContent = """
        #define MyAppName "Product Manager for DAZ Studio"
        #define MyAppVersion "0.9.8"
        #define MyAppPublisher ""Solomon Blount""
        #define MyAppURL "https://www.thesolomonchronicles.com"
        #define MyAppExeName "DAZ_Installer.Windows.exe"
        """;

        [TestMethod]
        public void UpdateVersion_ValidInput_ReturnsUpdatedContent()
        {
            // Arrange
            string content = SampleContent;
            string newVersion = "1.0.0";
            string suffix = ""; // Not used in the current implementation

            // Act
            string result = ISSVersionUpdater.UpdateVersion(content, newVersion, suffix);

            // Assert
            StringAssert.Contains(result, $"#define MyAppVersion \"{newVersion}\"");
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"#define MyAppVersion ""0\.9\.8"""));

            Debug.WriteLine("Got the following result:");
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void UpdateVersion_ValidInput_ReturnsUpdatedContent_LongVersion()
        {
            // Arrange
            string content = SampleContent;
            string newVersion = "9.99.9901";
            string suffix = ""; // Not used in the current implementation

            // Act
            string result = ISSVersionUpdater.UpdateVersion(content, newVersion, suffix);

            // Assert
            StringAssert.Contains(result, $"#define MyAppVersion \"{newVersion}\"");
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"#define MyAppVersion ""0\.9\.8"""));

            Debug.WriteLine("Got the following result:");
            Debug.WriteLine(result);
        }
    }

}

