using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Common.Net
{
    public class WidgetUpdatePacket : IPacket
    {
        public int Id { get; } = Protocol.WidgetUpdate;

        public int WidgetId { get; set; }

        public object[] Elements { get; set; }

    }
}
