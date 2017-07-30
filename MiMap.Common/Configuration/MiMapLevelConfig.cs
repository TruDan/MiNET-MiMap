using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Common.Configuration
{
    public class MiMapLevelConfig
    {
        public string LevelId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public int DefaultZoom { get; set; } = 0;

        public int MinZoom { get; set; } = 0;

        public int MaxZoom { get; set; } = 2;

        public int TileSize { get; set; } = 128;
        
        public MiMapRendererConfig Renderer { get; set; } = new MiMapRendererConfig();

        public MiMapLevelLayerConfig[] Layers { get; set; } = new MiMapLevelLayerConfig[0];
    }
}