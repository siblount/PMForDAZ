using System;
using System.Collections.Generic;

namespace DAZ_Installer.Core {
    public class DPDSXElementCollection
    {
        public Dictionary<string, LinkedList<DPDSXElement>> selfClosingElements = new Dictionary<string, LinkedList<DPDSXElement>>(3);
        public Dictionary<string, LinkedList<DPDSXElement>> nonSelfClosingElements = new Dictionary<string, LinkedList<DPDSXElement>>(3);
        private int count = 0;
        public void AddElement(DPDSXElement element)
        {
            count++;
            var newLinkedList = new LinkedList<DPDSXElement>();
            if (element.IsSelfClosingElement)
            {
                if (!selfClosingElements.TryAdd(element.TagName, newLinkedList)) {
                    selfClosingElements[element.TagName].AddLast(element);
                }
                newLinkedList.AddFirst(element);
            }
            else
            {
                if (!nonSelfClosingElements.TryAdd(element.TagName, newLinkedList)) {
                    nonSelfClosingElements[element.TagName].AddLast(element);
                } else {
                    newLinkedList.AddFirst(element);
                }
            }
        }

        public DPDSXElement[] GetAllElements()
        {
            List<DPDSXElement> elements = new List<DPDSXElement>(count);
            foreach (var list in selfClosingElements.Values) {
                elements.AddRange(list);
            }
            foreach (var list in nonSelfClosingElements.Values) {
                elements.AddRange(list);
            }
            return elements.ToArray();
        }

        public DPDSXElement[] FindElementViaTag(string tagName)
        {
            if (nonSelfClosingElements.ContainsKey(tagName)) {
                return new List<DPDSXElement>(nonSelfClosingElements[tagName]).ToArray();
            }
            if (selfClosingElements.ContainsKey(tagName)) {
                return new List<DPDSXElement>(selfClosingElements[tagName]).ToArray();
            }
            return Array.Empty<DPDSXElement>();
        }
    }
}
