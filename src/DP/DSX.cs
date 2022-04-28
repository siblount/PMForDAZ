// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.Buffers.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DP
{
    internal class DSXParser
    {
        protected readonly FileStream stream;
        protected int lastIndex = 0;
        protected DSXFile workingFile;
        protected const int bufferSize = 32768; // 32 KB
        protected int iteration = 0;
        internal bool hasErrored;
        protected Task asyncTask { get; set; } = null;
        internal DSXParser(string path)
        {
            try
            {
                stream = new FileStream(path, FileMode.Open);
                workingFile = new DSXFile();
                asyncTask = new Task(ReadFile);
                asyncTask.Start();
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog(e);
                hasErrored = true;
            }
        }
        ~DSXParser()
        {
            stream.Dispose();
        }

        protected void ReadFile()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            DPCommon.WriteToLog($"Reading file {stream.Name}...");
            if (stream.CanRead)
            {
                var bytes = new byte[bufferSize];
                var pendingBytes = new byte[] { };
                DSXElement lastElement = null;
                while (stream.Read(bytes, 0, bytes.Length) != 0)
                {

                    var asciiString = Encoding.ASCII.GetChars(bytes);
                    var tmp = GetNextElement(ref asciiString, 0, out lastIndex);
                    if (lastElement != null && tmp != null)
                    {
                        workingFile.AddElement(tmp);
                        lastElement.nextSibling = tmp;
                        tmp.previousSibling = lastElement;
                        lastElement = tmp;
                    }
                    else if (lastElement == null && tmp != null)
                    {
                        lastElement = tmp;
                        workingFile.AddElement(tmp);
                    }
                    while (lastElement != null)
                    {
                        var nextIndex = lastIndex;
                        var nextElement = GetNextElement(ref asciiString, nextIndex, out nextIndex);
                        lastElement.nextSibling = nextElement;
                        if (nextElement != null)
                        {
                            workingFile.AddElement(nextElement);
                            nextElement.previousSibling = lastElement;
                        }
                        lastIndex = nextIndex;
                        lastElement = nextElement;
                    }
                    iteration++;
                }

            }
            else hasErrored = true;
            watch.Stop();
            DPCommon.WriteToLog($"Execution Time: {watch.ElapsedMilliseconds} ms");
            foreach (var element in workingFile.GetAllElements())
            {
                DPCommon.WriteToLog($"Element Tag Name: {new string(element.tagName)}");
                foreach (var attribute in element.attributes)
                {
                    DPCommon.WriteToLog($"Attribute Name: {attribute.Key} | Attribute Value: {attribute.Value}");
                }
            }
        }

        internal int GetTotalIndex(int localIndex)
        {
            return iteration * bufferSize + localIndex;
        }

        internal char[] GetTotalMessage(int beginning, int end)
        {
            stream.Seek(beginning, SeekOrigin.Begin);
            var buffer = new byte[end - beginning];
            stream.Read(buffer, beginning, end - beginning);
            stream.Seek(end, SeekOrigin.Begin);
            return Encoding.ASCII.GetChars(buffer);
        }

        /// <summary>
        /// Iterates through given char array starting at offset.
        /// </summary>
        /// <returns>Returns the string of the next element OR returns null. Also returns the lastIndex.</returns>
        protected DSXElement GetNextElement(ref char[] arr, int offset, out int lastIndex)
        {
            try
            {
                lastIndex = -1;
                var workingElement = new DSXElement();
                var stringBuild = new List<char>();
                var attributeName = new char[] { };
                var attributeValue = new List<char>();
                var tagName = new List<char>();
                var isInTagCaptureMode = true;
                var isInQuoteMode = false;
                var isInAttributeMode = false;
                var isInDiscoverMode = false;
                var nextLessThanIndex = GetNextLessThan(ref arr, offset);
                var nextMoreThanIndex = -1;
                if (nextLessThanIndex != -1)
                {
                    stringBuild.Add('<');
                    for (var i = nextLessThanIndex + 1; i < arr.Length; i++)
                    {
                        lastIndex = i;
                        var c = arr[i];
                        if (!isInDiscoverMode && nextMoreThanIndex != -1 || nextLessThanIndex != -1)
                        {
                            stringBuild.Add(c);
                        }
                        if (c == '<' && !isInQuoteMode && !isInAttributeMode)
                        {
                            // Broken element.
                            return null;
                        }
                        else if (c == '"' || c == '\'')
                        {

                            if (isInAttributeMode && isInQuoteMode)
                            {
                                isInAttributeMode = false;
                                var result = workingElement.attributes.TryAdd(new string(attributeName), new string(attributeValue.ToArray()));
                                if (result == false) return null;
                                attributeName = new char[] { };
                                attributeValue = new List<char>();
                            }
                            isInQuoteMode = !isInQuoteMode;
                            continue;
                        }
                        else if (c == '=' && !isInQuoteMode)
                        {
                            isInAttributeMode = true;
                            // Go back and get attribute name until whitespace, ", ', =
                            attributeName = GetAttributeName(ref arr, i, nextLessThanIndex);
                        }
                        else if (c == '>' && !isInQuoteMode)
                        {
                            nextMoreThanIndex = i;
                            if (arr[i - 1] == '/') workingElement.isSelfClosing = true;
                            break;
                        }
                        if (isInAttributeMode && isInQuoteMode)
                        {
                            attributeValue.Add(c);
                        }
                        else if (isInTagCaptureMode)
                        {
                            if (!char.IsWhiteSpace(c) && !char.IsControl(c) && !char.IsSymbol(c) && !char.IsSurrogate(c) && c != '/') tagName.Add(c);
                            else
                            {
                                if (c == '/' && i - 1 == nextLessThanIndex) isInDiscoverMode = true;
                                isInTagCaptureMode = false;
                            }
                        }
                    }
                    workingElement.tagName = tagName.ToArray();
                    workingElement.beginningIndex = GetTotalIndex(nextLessThanIndex);
                    workingElement.endIndex = GetTotalIndex(nextMoreThanIndex);
                    workingElement.file = workingFile;

                    if (arr[lastIndex - 1] == '/') workingElement.messageIncludesEnding = true;
                    if (isInDiscoverMode)
                    {
                        var ourTagName = arr[(nextLessThanIndex + 2)..lastIndex];
                        var ourElement = workingFile.FindElementViaTag(ourTagName);
                        if (ourElement != null)
                        {
                            ourElement.messageIncludesEnding = true;
                            ourElement.totalMessage = GetTotalMessage(ourElement.beginningIndex, lastIndex);
                            var closingTagBeginningIndex = GetClosingTagLessThan(ref ourElement.totalMessage, ourElement.totalMessage.Length);
                            var beginningTagEndIndex = GetNextMoreThan(ref ourElement.totalMessage, 0);
                            ourElement.innerText = ourElement.totalMessage[(beginningTagEndIndex + 1)..closingTagBeginningIndex];
                            ourElement.endIndex = GetTotalIndex(nextMoreThanIndex);
                            ourElement.ParentChildrenWithinIndexRange();
                        }
                        return null;
                    }
                }
                if (nextLessThanIndex == -1) return null;
                return workingElement;
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog(e);
            }
            lastIndex = -1;
            return null;
        }

        /// <summary>
        /// The function goes back and continusually adds to it's string builder until it hits whitespace, a quote (" & '), =, or any other non-ASCII character.
        /// </summary>
        /// <param name="arr">The array that the function will access.</param>
        /// <param name="offset">The index at which the function should begin.</param>
        /// <param name="min">The index at which the function should end. </param>
        /// <returns>A string in the form of a char array or NULL if nothing is found.</returns>
        protected static char[] GetAttributeName(ref char[] arr, int offset, int min)
        {
            var stringBulder = new List<char>();
            for (var i = offset - 1; i > min; i--)
            {
                var c = arr[i];
                if (char.IsWhiteSpace(c) || char.IsSymbol(c) || char.IsSeparator(c) || char.IsSurrogate(c))
                {
                    break;
                }
                stringBulder.Add(c);
            }
            if (stringBulder.Count == 0) return null;
            stringBulder.Reverse();
            return stringBulder.ToArray();
        }

        protected static int GetNextLessThan(ref char[] arr, int offset)
        {
            var isInQuote = false;
            for (var i = offset; i < arr.Length; i++)
            {
                if (arr[i] == '"' || arr[i] == '\"' && !isInQuote)
                {
                    isInQuote = true;
                    continue;
                }
                else if (arr[i] == '"' || arr[i] == '\"' && isInQuote)
                {
                    isInQuote = false;
                    continue;
                }
                else if (arr[i] == '<')
                {
                    return i;
                }
            }
            return -1;
        }

        protected static int GetNextMoreThan(ref char[] arr, int offset)
        {
            var isInQuote = false;
            for (var i = offset; i < arr.Length; i++)
            {
                if (arr[i] == '"' || arr[i] == '\"' && !isInQuote)
                {
                    isInQuote = true;
                    continue;
                }
                else if (arr[i] == '"' || arr[i] == '\"' && isInQuote)
                {
                    isInQuote = false;
                    continue;
                }
                else if (arr[i] == '>')
                {
                    return i;
                }
            }
            return -1;
        }
        protected static int GetClosingTagLessThan(ref char[] arr, int offset)
        {
            for (var i = offset - 1; i > 0; i--)
            {
                if (arr[i] == '<') return i;
            }
            return -1;
        }

        internal DSXFile GetDSXFile()
        {
            asyncTask.Wait();
            return workingFile;
        }

    }

    internal class DSXFile
    {
        internal List<DSXElement> selfClosingElements = new List<DSXElement>();
        internal List<DSXElement> nonSelfClosingElements = new List<DSXElement>();

        internal void AddElement(DSXElement element)
        {
            if (element.isSelfClosing)
            {
                selfClosingElements.Add(element);
            }
            else
            {
                nonSelfClosingElements.Add(element);
            }
        }

        internal DSXElement[] GetAllElements()
        {
            return selfClosingElements.Concat(nonSelfClosingElements).ToArray();
        }

        internal DSXElement FindElementViaTag(char[] tagName)
        {
            foreach (var element in nonSelfClosingElements)
            {
                if (!element.messageIncludesEnding)
                {
                    var string1 = new string(element.tagName);
                    var string2 = new string(tagName);
                    if (string1 == string2) return element;
                }

            }
            return null;
        }
    }
    internal class DSXElement
    {
        internal readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
        internal char[] innerText = new char[] { };
        internal char[] tagName = new char[] { };
        internal char[] totalMessage = new char[] { };
        internal bool isSelfClosing = false;
        internal bool messageIncludesEnding = false;
        internal int beginningIndex = -1;
        internal int endIndex = -1;

        internal DSXElement parent;
        internal DSXElement nextSibling;
        internal DSXElement previousSibling;
        internal List<DSXElement> children = new List<DSXElement>();
        internal DSXFile file;
        internal DSXElement() { }
        /// <summary>
        /// DSXElements within the given index range will have its parent set to this DSXElement and added to children array.
        /// </summary>
        /// <param name="beginningIndex">The total beginning index of the buffer array.</param>
        /// <param name="endIndex">The total end index of the buffer array.</param>
        internal void ParentChildrenWithinIndexRange()
        {
            var workingSibling = nextSibling;
            while (workingSibling != null && IndexInRange(beginningIndex, endIndex, ref workingSibling))
            {
                workingSibling.parent = this;
                children.Add(workingSibling);
                workingSibling = workingSibling.nextSibling;
            }
        }

        protected static bool IndexInRange(int beginningIndex, int endIndex, ref DSXElement element)
        {
            if (element.beginningIndex > beginningIndex && element.endIndex < endIndex) return true;
            else return false;
        }

    }
}
