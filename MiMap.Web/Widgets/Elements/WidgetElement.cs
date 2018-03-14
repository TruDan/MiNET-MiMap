using System;
using log4net;
using MiMap.Common;
using MiMap.Common.Net;
using MiMap.Web.Sockets;
using Newtonsoft.Json;

namespace MiMap.Web.Widgets.Elements
{
    public class WidgetElement
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WidgetElement));

        [JsonIgnore]
        public Widget Widget { get; }

        [JsonIgnore]
        public WidgetElement Parent { get; }

        public int ElementId { get; }

        public virtual string ElementType { get; } = "";


        public WidgetElement(WidgetElement parent)
        {
            Parent = parent;
            Widget = parent?.Widget ?? parent as Widget ?? this as Widget;

            if (Widget != null)
            {
                ElementId = Widget.NextElementId();
            }
            else
            {
                throw new Exception("Widget Element does not have a parent widget.");
            }
        }


        protected virtual void Update()
        {
            // Send data
            WsServer.BroadcastPacket(new WidgetElementUpdatePacket()
            {
                WidgetId = Widget.WidgetId,
                ElementId = ElementId,
                Data = this
            });
        }

        protected virtual void OnInput(string json)
        {
            Log.WarnFormat("Unhandled Widget Input for type {0} with ID {1}:\n\t{2}", GetType().Name, ElementId, MiMapJsonConvert.SerializeObject(this));
        }

    }
}
