using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MiMap.Common;
using MiMap.Common.Configuration;

namespace MiMap.Web.Widgets
{
    public class WidgetManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WidgetManager));

        private Dictionary<int, Widget> Widgets { get; } = new Dictionary<int, Widget>();

        private int _widgetIndex;

        public WidgetManager(MiMapWidgetConfig[] widgets)
        {
            LoadWidgets(widgets);
        }

        internal void ConfigureHttp(IApplicationBuilder app)
        {
            app.Map("/widgets/config.json", builder =>
            {
                builder.Use(
                    (context, next) => context.Response.WriteAsync(MiMapJsonConvert.SerializeObject(Widgets.ToArray())));
            });
        }

        private void LoadWidgets(MiMapWidgetConfig[] widgets)
        {
            foreach (var widgetConfig in widgets)
            {
                LoadWidget(widgetConfig);
            }
        }

        private void LoadWidget(MiMapWidgetConfig widgetConfig)
        {
            var widget = WidgetFactory.CreateWidget(widgetConfig.Type);
            if (widget == null)
            {
                Log.ErrorFormat("Widget type invalid: {0}", widgetConfig.Type);
                return;
            }

            Widgets.Add(_widgetIndex++, widget);
        }

        private void Save()
        {

        }


    }
}
