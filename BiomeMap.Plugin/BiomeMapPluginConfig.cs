using BiomeMap.Shared.Configuration;

namespace BiomeMap.Plugin
{
    public class BiomeMapPluginConfig : BiomeMapConfig
    {

        public bool WebServerEnabled { get; set; } = true;

        public string WebServerPath { get; set; } = string.Empty;

    }

}
