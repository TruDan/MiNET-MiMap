using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Web.Widgets;
using Owin;

namespace MiMap.Widgets.RuntimeStats
{

    [Widget("Performance")]
    public class PerformanceWidget : Widget, IHttpWidget
    {
        public PerformanceWidget() : base("Performance")
        {
            AddTextElement("Chunks Processed");
            AddTextElement("Tiles Generated");
            AddTextElement("Tile Directory Size");

            AddTextElement("Tiles Generated");
        }
    }
}
