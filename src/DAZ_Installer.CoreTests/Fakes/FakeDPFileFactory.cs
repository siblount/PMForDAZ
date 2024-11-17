using DAZ_Installer.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IDPFileFactory"/> that can be used for testing.
    /// </summary>
    public class FakeDPFileFactory : IDPFileFactory
    {
        /// <summary>
        /// Determines if the mock should be a partial mock or not.
        /// </summary>
        /// <remarks>In other words, determines the <see cref="Mock.CallBase"/> value.</remarks>
        public virtual bool PartialMock { get; set; } = true;
        /// <summary>
        /// Creates a new instance of <see cref="FakeDPFileFactory"/>.
        /// </summary>
        /// <param name="path">The raw path to set for the file.</param>
        /// <param name="arc">The archive to associate with this file, if any.</param>
        /// <param name="parent">The parent of the file, if any.</param>
        /// <returns>A <see cref="FakeDPFile"/></returns>
        public virtual IDPFile CreateNewFile(string path, IDPArchive? arc, IDPFolder? parent) => new Mock<FakeDPFile>(path) { CallBase = PartialMock }.Object;

        public FakeDPFileFactory() { }
        public FakeDPFileFactory(bool partialMock) => PartialMock = partialMock;
    }
}
