using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Web.Widgets
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetAttribute : Attribute
    {
        public string Name { get; set; }

        public WidgetAttribute(string name)
        {
            Name = name;
        }


    }
}
