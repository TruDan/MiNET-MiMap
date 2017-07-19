using BiomeMap.Drawing;
using BiomeMap.Shared;

namespace BiomeMap
{
    public class BiomeMapPluginConfig : BiomeMapConfig
    {

        public bool WebServerEnabled { get; set; } = true;

        public string WebServerPath { get; set; } = string.Empty;

    }

}
