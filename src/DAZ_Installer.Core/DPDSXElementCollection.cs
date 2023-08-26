namespace DAZ_Installer.Core
{
    public class DPDSXElementCollection
    {
        public Dictionary<string, LinkedList<DPDSXElement>> selfClosingElements = new(3);
        public Dictionary<string, LinkedList<DPDSXElement>> nonSelfClosingElements = new(3);
        public int Count { get; private set; } = 0;
        public void AddElement(DPDSXElement element)
        {
            Count++;
            var newLinkedList = new LinkedList<DPDSXElement>();
            if (element.IsSelfClosingElement)
            {
                if (!selfClosingElements.TryAdd(element.TagName, newLinkedList))
                {
                    selfClosingElements[element.TagName].AddLast(element);
                }
                newLinkedList.AddFirst(element);
            }
            else
            {
                if (!nonSelfClosingElements.TryAdd(element.TagName, newLinkedList))
                {
                    nonSelfClosingElements[element.TagName].AddLast(element);
                }
                else
                {
                    newLinkedList.AddFirst(element);
                }
            }
        }


        public IEnumerable<DPDSXElement> GetAllElements() => selfClosingElements.Values.SelectMany(list => list)
            .Concat(nonSelfClosingElements.Values.SelectMany(list => list));

        public List<DPDSXElement> FindElementViaTag(string tagName)
        {
            if (nonSelfClosingElements.ContainsKey(tagName))
            {
                return new List<DPDSXElement>(nonSelfClosingElements[tagName]);
            }
            if (selfClosingElements.ContainsKey(tagName))
            {
                return new List<DPDSXElement>(selfClosingElements[tagName]);
            }
            return new List<DPDSXElement>(0);
        }
    }
}
