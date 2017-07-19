using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiomeMap.Shared
{
    public class BiomeMapConfig
    {

        public string TilesDirectory { get; set; } =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "BiomeMapManager");

        public int SaveInterval { get; set; } = 5000;

        public BiomeMapLevelConfig[] Levels { get; set; }
    }

    public enum BiomeMapLayerRenderer
    {
        Default,
        Texture,
        SolidColor
    }

    public enum BiomeMapOverlayRenderer
    {
        BiomeFoilage,
        BiomeTemperature,
        HeightMap,
    }

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

    public class BiomeMapLevelLayerConfig
    {
        public string LayerId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Default { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public BlendMode BlendMode { get; protected set; } = BlendMode.Normal;

        [JsonConverter(typeof(StringEnumConverter))]
        public BiomeMapOverlayRenderer Renderer { get; set; }
    }
}
