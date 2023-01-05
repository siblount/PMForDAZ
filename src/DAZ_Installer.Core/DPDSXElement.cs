// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;

namespace DAZ_Installer.Core {
    internal class DPDSXElement
    {
        internal readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
        internal Memory<char> InnerText = new char[] { };
        internal string TagName = string.Empty;
        internal Memory<char> TotalMessage = new char[] { };
        internal bool IsSelfClosingElement = false;
        internal bool MessageIncludesEnding = false;
        internal int BeginningIndex = -1;
        internal int EndIndex = -1;

        internal DPDSXElement Parent;
        internal DPDSXElement NextSibling;
        internal DPDSXElement PreviousSibling;
        internal List<DPDSXElement> Children = new List<DPDSXElement>();
        internal DPDSXElementCollection File;
        internal DPDSXElement() { }
        /// <summary>
        /// DSXElements within the given index range will have its parent set to this DSXElement and added to children array.
        /// </summary>
        /// <param name="beginningIndex">The total beginning index of the buffer array.</param>
        /// <param name="endIndex">The total end index of the buffer array.</param>
        internal void ParentChildrenWithinIndexRange()
        {
            var workingSibling = NextSibling;
            while (workingSibling != null && IndexInRange(BeginningIndex, EndIndex, workingSibling))
            {
                workingSibling.Parent = this;
                Children.Add(workingSibling);
                workingSibling = workingSibling.NextSibling;
            }
        }

        protected static bool IndexInRange(int beginningIndex, int endIndex, DPDSXElement element)
        {
            if (element.BeginningIndex > beginningIndex && element.EndIndex < endIndex) return true;
            else return false;
        }

    }
}    
    