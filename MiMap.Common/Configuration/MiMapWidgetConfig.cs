using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Common.Configuration
{
    public class MiMapWidgetConfig
    {
        public string Type { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        public WidgetPosition Position { get; set; } = WidgetPosition.TopRight;

        public WidgetPosition[] AllowedPositions { get; set; } = new WidgetPosition[0];


        public bool Moveable { get; set; } = false;

        public bool Collapsible { get; set; } = false;


        public bool IsCollapsed { get; set; } = false;
    }
}
