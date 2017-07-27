using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiomeMap.Common.Configuration
{
    public class BiomeMapLevelLayerConfig
    {
        public string LayerId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Default { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public BlendMode BlendMode { get; set; } = BlendMode.Normal;

        [JsonConverter(typeof(StringEnumConverter))]
        public BiomeMapOverlayRenderer Renderer { get; set; }
    }
}