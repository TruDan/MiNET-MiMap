using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Common.Net
{
    public class WidgetElementUpdatePacket : IPacket
    {
        public int Id { get; } = Protocol.WidgetElementUpdate;

        public int WidgetId { get; set; }
        public int ElementId { get; set; }
        public object Data { get; set; }
    }
}
