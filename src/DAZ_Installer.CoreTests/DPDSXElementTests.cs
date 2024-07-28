using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System.Collections.Generic;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPDSXElementTests
    {
        [TestMethod]
        public void ParentChildrenWithinIndexRange_NoSiblingsWithinRange_NoChildrenAdded()
        {
            // Arrange
            var parent = new DPDSXElement { BeginningIndex = 0, EndIndex = 10 };
            var sibling = new DPDSXElement { BeginningIndex = 15, EndIndex = 20 };
            parent.NextSibling = sibling;

            // Act
            parent.ParentChildrenWithinIndexRange();

            // Assert
            Assert.AreEqual(0, parent.Children.Count);
            Assert.IsNull(sibling.Parent);
        }

        [TestMethod]
        public void ParentChildrenWithinIndexRange_AllSiblingsWithinRange_AllChildrenAdded()
        {
            // Arrange
            var parent = new DPDSXElement { BeginningIndex = 0, EndIndex = 30 };
            var sibling1 = new DPDSXElement { BeginningIndex = 5, EndIndex = 10 };
            var sibling2 = new DPDSXElement { BeginningIndex = 15, EndIndex = 20 };
            parent.NextSibling = sibling1;
            sibling1.NextSibling = sibling2;

            // Act
            parent.ParentChildrenWithinIndexRange();

            // Assert
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(parent, sibling1.Parent);
            Assert.AreEqual(parent, sibling2.Parent);
            CollectionAssert.Contains(parent.Children, sibling1);
            CollectionAssert.Contains(parent.Children, sibling2);
        }

        [TestMethod]
        public void ParentChildrenWithinIndexRange_SomeSiblingsWithinRange_OnlyWithinRangeChildrenAdded()
        {
            // Arrange
            var parent = new DPDSXElement { BeginningIndex = 0, EndIndex = 25 };
            var sibling1 = new DPDSXElement { BeginningIndex = 5, EndIndex = 10 };
            var sibling2 = new DPDSXElement { BeginningIndex = 15, EndIndex = 20 };
            var sibling3 = new DPDSXElement { BeginningIndex = 30, EndIndex = 35 };
            parent.NextSibling = sibling1;
            sibling1.NextSibling = sibling2;
            sibling2.NextSibling = sibling3;

            // Act
            parent.ParentChildrenWithinIndexRange();

            // Assert
            Assert.AreEqual(2, parent.Children.Count);
            Assert.AreEqual(parent, sibling1.Parent);
            Assert.AreEqual(parent, sibling2.Parent);
            Assert.IsNull(sibling3.Parent);
            CollectionAssert.Contains(parent.Children, sibling1);
            CollectionAssert.Contains(parent.Children, sibling2);
            CollectionAssert.DoesNotContain(parent.Children, sibling3);
        }

        [TestMethod]
        public void ParentChildrenWithinIndexRange_SiblingOnBoundary_NotAdded()
        {
            // Arrange
            var parent = new DPDSXElement { BeginningIndex = 0, EndIndex = 20 };
            var sibling1 = new DPDSXElement { BeginningIndex = 5, EndIndex = 10 };
            var sibling2 = new DPDSXElement { BeginningIndex = 20, EndIndex = 25 };
            parent.NextSibling = sibling1;
            sibling1.NextSibling = sibling2;

            // Act
            parent.ParentChildrenWithinIndexRange();

            // Assert
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(parent, sibling1.Parent);
            Assert.IsNull(sibling2.Parent);
            CollectionAssert.Contains(parent.Children, sibling1);
            CollectionAssert.DoesNotContain(parent.Children, sibling2);
        }
    }
}
