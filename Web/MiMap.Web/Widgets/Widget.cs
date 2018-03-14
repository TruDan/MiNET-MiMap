using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Common.Configuration;
using MiMap.Common.Enums;
using MiMap.Web.Widgets.Elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Web.Widgets
{
    public abstract class Widget : WidgetElementGroup, IWidget
    {
        protected WidgetManager WidgetManager { get; private set; }

        public int WidgetId { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WidgetPosition Position { get; set; }

        public MiMapWidgetConfig Config { get; set; }

        private int _elementIndex;


        protected Widget(string label) : base(null, label)
        {
        }

        internal void Initialise(WidgetManager widgetManager, int widgetId, MiMapWidgetConfig config)
        {
            WidgetManager = widgetManager;
            WidgetId = widgetId;

            Config = config;
            {
                Collapsible = config.Collapsible;
                IsCollapsed = config.IsCollapsed;
                Position = config.Position;
            }

            OnInitialise();
        }

        protected virtual void OnInitialise() { }

        internal int NextElementId()
        {
            return _elementIndex++;
        }


    }
}
