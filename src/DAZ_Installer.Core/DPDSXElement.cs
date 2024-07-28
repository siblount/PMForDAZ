// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System.Runtime.CompilerServices;

namespace DAZ_Installer.Core
{
    public class DPDSXElement
    {
        public readonly Dictionary<string, string> attributes = new();
        public Memory<char> InnerText = Array.Empty<char>();
        public string TagName = string.Empty;
        public Memory<char> TotalMessage = Array.Empty<char>();
        public bool IsSelfClosingElement = false;
        public bool MessageIncludesEnding = false;
        public int BeginningIndex = -1;
        public int EndIndex = -1;

        public DPDSXElement? Parent;
        public DPDSXElement? NextSibling;
        public DPDSXElement? PreviousSibling;
        public List<DPDSXElement> Children = new();
        public DPDSXElementCollection File;
        public DPDSXElement() { }
        /// <summary>
        /// DSXElements within the given index range will have its parent set to this DSXElement and added to children array.
        /// </summary>
        public void ParentChildrenWithinIndexRange()
        {
            DPDSXElement? workingSibling = NextSibling;
            while (workingSibling != null && IndexInRange(BeginningIndex, EndIndex, workingSibling))
            {
                workingSibling.Parent = this;
                Children.Add(workingSibling);
                workingSibling = workingSibling.NextSibling;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IndexInRange(int beginningIndex, int endIndex, DPDSXElement element)
        {
            return (element.BeginningIndex > beginningIndex && element.EndIndex < endIndex);
        }

    }
}
