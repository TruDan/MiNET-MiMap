using System.Collections.Generic;

namespace MiMap.Web.Widgets.Elements
{
    public class WidgetElementGroup : WidgetElement
    {
        public string Title { get; }

        public bool Collapsible { get; set; }

        public bool IsCollapsed { get; set; }

        private LinkedList<WidgetElement> Elements { get; }


        public WidgetElementGroup(WidgetElement parent, string title) : base(parent)
        {
            Title = title;
            Elements = new LinkedList<WidgetElement>();
        }

        public void AddElement(WidgetElement element)
        {
            Elements.AddLast(element);
        }

        public TextElement AddTextElement(string label, string text = "--")
        {
            var e = new TextElement(this, label, text);
            AddElement(e);
            return e;
        }
    }
}
