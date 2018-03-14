using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Web.Widgets.Elements
{
    public class TextElement : WidgetElement
    {

        public string Label { get; }

        public string Text { get; set; }

        public override string ElementType { get; } = "text";

        public TextElement(WidgetElement parent, string label, string text = "") : base(parent)
        {
            Label = label;
            Text = text;
        }
    }
}
