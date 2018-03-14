using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Common.Configuration;
using MiMap.Common.Enums;

namespace MiMap.Web.Widgets
{
    public interface IWidget
    {
        int WidgetId { get; }

        WidgetPosition Position { get; set; }

        MiMapWidgetConfig Config { get; set; }


    }
}
