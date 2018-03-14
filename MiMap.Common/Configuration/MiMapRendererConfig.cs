using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Common.Configuration
{
    public class MiMapRendererConfig
    {

        public string Type { get; set; } = "Default";

        public string[] PostProcessors { get; set; } = { "Lighting", "HeightShadow" };

        [JsonExtensionData]
        public IDictionary<string, object> Options { get; set; } = new Dictionary<string, object>();

    }
}
