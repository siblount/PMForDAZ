// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DP
{
    internal class DPDSXParser
    {
        protected readonly StreamReader stream;
        protected int lastIndex = 0;
        protected DPDSXElementCollection workingFileContents = new DPDSXElementCollection();
        protected const int BUFFER_SIZE = 16384; // in chars. 32 KB.
        protected int chunk = 0;
        internal bool hasErrored;
        internal string fileName = string.Empty;
        protected Task asyncTask { get; set; } = null;
        internal DPDSXParser(string path)
        {
            fileName = Path.GetFileName(fileName);
            try
            {
                stream = new StreamReader(path, Encoding.UTF8, true);
                asyncTask = new Task(ReadFile);
                asyncTask.Start();
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog(e);
                hasErrored = true;
            }
        }
        ~DPDSXParser()
        {
            stream.Dispose();
        }

        protected void ReadFile()
        {
            var watch = new System.Diagnostics.Stopwatch();
            int offset = 0;
            DPDSXElement lastElement = null;
            Span<char> chars = new char[BUFFER_SIZE];
            DPCommon.WriteToLog($"Reading file {fileName}...");
            watch.Start();
            while (stream.Read(chars) != 0) {
                var tmp = GetNextElement(chars, 0, out lastIndex);
                if (lastElement != null && tmp != null) {
                    workingFileContents.AddElement(tmp);
                    lastElement.NextSibling = tmp;
                    tmp.PreviousSibling = lastElement;
                    lastElement = tmp;
                } else if (lastElement == null && tmp != null) {
                    workingFileContents.AddElement(tmp);
                    lastElement = tmp;
                }
                while (lastElement != null) {
                    var nextIndex = lastIndex;
                    var element = GetNextElement(chars, nextIndex, out nextIndex);
                    lastElement.NextSibling = element;
                    if (element != null) {
                        workingFileContents.AddElement(element);
                        element.PreviousSibling = lastElement;
                    }
                    lastIndex = nextIndex;
                    lastElement = element;
                }
                chunk++;
                offset = chars.Length - 1 - lastIndex;
            }
            watch.Stop();
            DPCommon.WriteToLog($"Execution Time: {watch.ElapsedMilliseconds} ms");
            foreach (var element in workingFileContents.GetAllElements())
            {
                DPCommon.WriteToLog($"Element Tag Name: {new string(element.TagName)}");
                foreach (var attribute in element.attributes)
                {
                    DPCommon.WriteToLog($"Attribute Name: {attribute.Key} | Attribute Value: {attribute.Value}");
                }
            }
        }

        /// <summary>
        /// Iterates through given char array starting at offset.
        /// </summary>
        /// <returns>Returns the string of the next element OR returns null. Also returns the lastIndex.</returns>
        protected DPDSXElement? GetNextElement(ReadOnlySpan<char> arr, int offset, out int lastIndex)
        {
            Memory<char> totalMessage = Memory<char>.Empty; // includes < and >
            lastIndex = -1;
            var workingElement = new DPDSXElement();
            string attributeName = string.Empty;
            var attributeValue = new StringBuilder(30);
            var tagName = new StringBuilder(30);
            var isInTagCaptureMode = true;
            var isInQuoteMode = false;
            var isInAttributeMode = false;
            var isInDiscoverMode = false;
            var nextLessThanIndex = GetNextLessThan(arr, offset); // Returns the index at '<'
            var nextMoreThanIndex = -1;
            try
            {
                if (nextLessThanIndex != -1)
                {
                    
                    // stringBuild.Add('<');
                    for (var i = nextLessThanIndex + 1; i < arr.Length; i++)
                    {
                        lastIndex = i;
                        var c = arr[i];
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
                                var result = workingElement.attributes.TryAdd(attributeName, attributeValue.ToString());
                                if (result == false) return null;
                                attributeName = string.Empty;
                                attributeValue.Clear();
                            }
                            isInQuoteMode = !isInQuoteMode;
                            continue;
                        }
                        else if (c == '=' && !isInQuoteMode)
                        {
                            isInAttributeMode = true;
                            // Go back and get attribute name until whitespace, ", ', =
                            attributeName = GetAttributeName(arr, i, nextLessThanIndex);
                        }
                        else if (c == '>' && !isInQuoteMode)
                        {
                            nextMoreThanIndex = i;
                            if (arr[i - 1] == '/') workingElement.IsSelfClosingElement = true;
                            break;
                        }
                        if (isInAttributeMode && isInQuoteMode)
                        {
                            attributeValue.Append(c);
                        }
                        else if (isInTagCaptureMode)
                        {
                            if (!char.IsWhiteSpace(c) && !char.IsControl(c) && !char.IsSymbol(c) && !char.IsSurrogate(c) && c != '/')
                                tagName.Append(c);
                            else
                            {
                                if (c == '/' && i - 1 == nextLessThanIndex) isInDiscoverMode = true;
                                isInTagCaptureMode = false;
                            }
                        }
                    }
                    
                    if (nextLessThanIndex == -1) {
                        lastIndex = -1;
                        return null;
                    }
                    totalMessage = new char[nextMoreThanIndex - nextLessThanIndex];
                    workingElement.TagName = tagName.ToString();
                    workingElement.BeginningIndex = nextLessThanIndex;
                    workingElement.EndIndex = nextMoreThanIndex;
                    workingElement.File = workingFileContents;
                    if (arr[lastIndex - 1] == '/') workingElement.MessageIncludesEnding = true;
                    if (isInDiscoverMode)
                    {
                        // var ourTagName = arr[(nextLessThanIndex + 2)..lastIndex];
                        // TODO: Check if ourTagName == workingElement.TagName
                        var ourTagName = arr.Slice(nextLessThanIndex + 2, lastIndex - nextLessThanIndex + 1);
                        var ourElements = workingFileContents.FindElementViaTag(new string(ourTagName));
                        if (ourElements?.Length != 0)
                        {
                            foreach (var element in ourElements) {
                                element.MessageIncludesEnding = true;
                                element.TotalMessage = totalMessage;
                                var closingTagBeginningIndex = GetClosingTagLessThan(element.TotalMessage.Span, element.TotalMessage.Length);
                                var beginningTagEndIndex = GetNextMoreThan(element.TotalMessage.Span, 0);
                                element.InnerText = element.TotalMessage.Slice((beginningTagEndIndex + 1), closingTagBeginningIndex - beginningTagEndIndex);
                                element.EndIndex = nextMoreThanIndex;
                                element.ParentChildrenWithinIndexRange();
                            }
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
        protected static string GetAttributeName(ReadOnlySpan<char> arr, int offset, int min)
        {
            var stringBuilder = new List<char>(5); // Average is VALUE (5). 
            for (var i = offset - 1; i > min; i--)
            {
                var c = arr[i];
                if (char.IsWhiteSpace(c) || char.IsSymbol(c) || char.IsSeparator(c) || char.IsSurrogate(c))
                {
                    break;
                }
                stringBuilder.Add(c);
            }
            if (stringBuilder.Count == 0) return null;
            stringBuilder.Reverse();
            return new string(stringBuilder.ToArray());
        }
        /// <summary>
        /// Returns the index of the next less than symbol (<) at starting at `offset`.
        /// </summary>
        /// <param name="arr">The character array to search.</param>
        /// <param name="offset">The starting offset of the array.</param>
        /// <returns></returns>
        protected static int GetNextLessThan(ReadOnlySpan<char> arr, int offset)
        {
            var isInQuote = false;
            for (var i = offset; i < arr.Length; i++)
            {
                if (arr[i] == '"')
                    isInQuote = !isInQuote;
                else if (arr[i] == '<')
                    return i;
            }
            return -1;
        }

        protected static int GetNextMoreThan(ReadOnlySpan<char> arr, int offset)
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
        protected static int GetClosingTagLessThan(ReadOnlySpan<char> arr, int offset)
        {
            for (var i = offset - 1; i > 0; i--)
            {
                if (arr[i] == '<') return i;
            }
            return -1;
        }

        internal DPDSXElementCollection GetDSXFile()
        {
            asyncTask.Wait();
            return workingFileContents;
        }

    }
}
