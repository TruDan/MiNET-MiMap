using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Common.Configuration
{
    public class MiMapWebServerConfig
    {

        public short Port { get; set; } = 8124;
        public short InternalWebPort { get; set; } = 8125;
        public short InternalWebSocketPort { get; set; } = 8126;

        public Boolean Enabled { get; set; } = true;

        public MiMapWidgetConfig[] Widgets { get; set; } = new MiMapWidgetConfig[0];

    }
}
