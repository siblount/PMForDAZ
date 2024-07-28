// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DAZ_Installer.Core
{
    [Flags]
    internal enum DPDSXElementFlags
    {
        None = 0,
        IsSelfClosingElement = 1,
        MessageIncludesEnding = 2,
        IsEndingElement = 4,
    }
    public class DPDSXElement
    {
        public readonly Dictionary<string, string> attributes = new();
        public Memory<char> InnerText = Array.Empty<char>();
        public string TagName = string.Empty;
        public Memory<char> TotalMessage = Array.Empty<char>();
        internal DPDSXElementFlags Flags = DPDSXElementFlags.None;
        /// <summary>
        /// Gets or sets a value indicating whether this element is a self-closing element.
        /// </summary>
        public bool IsSelfClosingElement
        {
            get => Flags.HasFlag(DPDSXElementFlags.IsSelfClosingElement);
            set => Flags = value ? Flags | DPDSXElementFlags.IsSelfClosingElement : Flags & ~DPDSXElementFlags.IsSelfClosingElement;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the message includes the ending of this element.
        /// </summary>
        public bool MessageIncludesEnding
        {
            get => Flags.HasFlag(DPDSXElementFlags.MessageIncludesEnding);
            set => Flags = value ? Flags | DPDSXElementFlags.MessageIncludesEnding : Flags & ~DPDSXElementFlags.MessageIncludesEnding;
        }
        /// <summary>
        /// Gets or sets a value indicating whether this element is an ending element.
        /// </summary>
        /// <remarks>
        /// An ending element is a closing tag such as &lt;/Root&gt; or &lt;/SomeRandomElement&gt;.
        /// <br/>
        /// Specifically, an ending element is a tag that starts with &lt;/ and ends with &gt;.
        /// </remarks>
        public bool IsEndingElement
        {
            get => Flags.HasFlag(DPDSXElementFlags.IsEndingElement);
            set => Flags = value ? Flags | DPDSXElementFlags.IsEndingElement : Flags & ~DPDSXElementFlags.IsEndingElement;
        }
        public int BeginningIndex = -1;
        public int EndIndex = -1;

        public DPDSXElement? Parent;
        public DPDSXElement? NextSibling;
        public DPDSXElement? PreviousSibling;
        public List<DPDSXElement> Children = new();
        public DPDSXElementCollection File;
        public DPDSXElement() { }
        /// <summary>
        /// Parents any children within the index range of this element.
        /// </summary>
        public void ParentChildrenWithinIndexRange()
        {
            DPDSXElement? workingSibling = NextSibling;
            while (CanParentChildren(workingSibling))
            {
                workingSibling.Parent = this;
                Children.Add(workingSibling);
                workingSibling = workingSibling.NextSibling;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanParentChildren([NotNullWhen(true)] DPDSXElement? element) => element is not null
                                                                                     && element.Parent is null
                                                                                     && IndexInRange(BeginningIndex, EndIndex, element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IndexInRange(int beginningIndex, int endIndex, DPDSXElement element)
        {
            return (element.BeginningIndex > beginningIndex && element.EndIndex < endIndex);
        }

    }
}
