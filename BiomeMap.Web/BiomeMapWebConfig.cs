using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiomeMap.Web
{
    public class BiomeMapWebConfig
    {
        public string ConfigPath { get; set; } =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "biomemap.json");
    }
}
