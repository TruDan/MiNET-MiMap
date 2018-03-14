using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MiMap.Web.Widgets
{
    public class WidgetFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WidgetFactory));

        private static readonly IDictionary<string, Type> WidgetTypes = new Dictionary<string, Type>();

        static WidgetFactory()
        {
            InitiialiseWidgetTypes();
        }

        private static void InitiialiseWidgetTypes()
        {
            string path = Path.GetDirectoryName(typeof(WidgetFactory).Assembly.Location);
            Log.InfoFormat("Scanning {0} for Widgets", path);

            foreach (var file in Directory.GetFiles(path, "MiMap.Widget.*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var a = Assembly.LoadFrom(file);
                    foreach (var c in a.GetTypes())
                    {
                        if (!typeof(Widget).IsAssignableFrom(c))
                            continue;

                        var attr = c.GetCustomAttribute(typeof(WidgetAttribute), true) as WidgetAttribute;
                        if (attr == null)
                        {
                            Log.InfoFormat("Widget {0} does not have a valid Widget Attribute!", c.FullName);
                            continue;
                        }

                        RegisterWidget(attr.Name, c);
                    }
                }
                catch (FileLoadException loadEx)
                { }
            }
        }

        public static void RegisterWidget(string id, Type type)
        {
            if (!type.IsClass || type.IsGenericType)
            {
                Log.ErrorFormat("Failed to register Widget type {0} - Not valid class or is generic.", type.FullName);
                return;
            }

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var cParams = constructor.GetParameters();
                if (cParams.Length == 0)
                {
                    WidgetTypes.Add(id.ToLowerInvariant(), type);
                    return;
                }
                else
                {
                    Log.WarnFormat("Invalid Widget constructor {0}::{1}", type.FullName, string.Join(", ", cParams.Select(p1 => p1.ParameterType.Name)));
                }
            }

            Log.ErrorFormat("Failed to register Widget type {0} - No Valid constructor", type.FullName);
        }

        public static Widget CreateWidget(string id)
        {
            Type type;
            if (WidgetTypes.TryGetValue(id.ToLowerInvariant(), out type))
            {
                return Activator.CreateInstance(type) as Widget;

            }
            return null;
        }
    }
}
