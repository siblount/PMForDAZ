using Microsoft.VisualStudio.TestTools.UnitTesting;
using Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [TestClass]
    public class CSProjVersionUpdaterTests
    {
        private const string TestProjectContent = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <TargetFramework>net6.0-windows</TargetFramework>
            <Version>$(AssemblyVersion)$(VersionSuffix)</Version>
            <FileVersion>0.9.8</FileVersion>
            <AssemblyVersion>0.9.8</AssemblyVersion>
          </PropertyGroup>
          <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
            <VersionSuffix>Pre-alpha</VersionSuffix>
          </PropertyGroup>
        </Project>
        """;

        [TestMethod]
        public void UpdateVersion_ShouldUpdateAllVersionElements()
        {
            // Arrange
            string newVersion = "1.0.0";
            string newSuffix = "Beta";

            // Act
            string updatedContent = CSProjVersionUpdater.UpdateVersion(TestProjectContent, newVersion, newSuffix);

            // Assert
            Assert.IsTrue(Regex.IsMatch(updatedContent, $@"<AssemblyVersion>1.0.0</AssemblyVersion>"));
            Assert.IsTrue(Regex.IsMatch(updatedContent, $@"<FileVersion>1.0.0</FileVersion>"));
            Assert.IsTrue(Regex.IsMatch(updatedContent, $@"<VersionSuffix>Beta</VersionSuffix>"));

            Debug.WriteLine("Got the following result:");
            Debug.WriteLine(updatedContent);
        }
    }

}