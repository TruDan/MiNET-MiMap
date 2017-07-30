using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Common.Configuration
{
    public class MiMapLevelLayerConfig
    {
        public string LayerId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Default { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public BlendMode BlendMode { get; set; } = BlendMode.Normal;
        
        public MiMapRendererConfig Renderer { get; set; } = new MiMapRendererConfig();
    }
}