using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiomeMap.Common.Configuration
{
    public class BiomeMapLevelConfig
    {
        public string LevelId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public int DefaultZoom { get; set; } = 0;

        public int MinZoom { get; set; } = 0;

        public int MaxZoom { get; set; } = 2;

        public int TileSize { get; set; } = 128;

        [JsonConverter(typeof(StringEnumConverter))]
        public BiomeMapLayerRenderer BaseLayer { get; set; } = BiomeMapLayerRenderer.Default;

        public BiomeMapLevelLayerConfig[] Layers { get; set; } = new BiomeMapLevelLayerConfig[0];
    }
}