using MiMap.Common.Configuration;

namespace MiMap.Plugin
{
    public class MiMapPluginConfig : MiMapConfig
    {

        public bool WebServerEnabled { get; set; } = true;

        public string WebServerPath { get; set; } = string.Empty;

    }

}
