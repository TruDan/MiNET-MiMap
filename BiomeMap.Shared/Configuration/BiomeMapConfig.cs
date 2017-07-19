using System.IO;
using System.Reflection;

namespace BiomeMap.Shared.Configuration
{
    public class BiomeMapConfig
    {

        public string TilesDirectory { get; set; } =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "BiomeMapManager");

        public int SaveInterval { get; set; } = 2500;

        public BiomeMapLevelConfig[] Levels { get; set; }
    }
}
