using System;
using System.IO;
using System.Reflection;
using log4net;
using MiNET;
using Newtonsoft.Json;

namespace BiomeMap.Common.Configuration
{
    public class BiomeMapConfig
    {
        [JsonIgnore]
        public static BiomeMapConfig Config { get; }

        [JsonIgnore]
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapConfig));

        public string TilesDirectory { get; set; } =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "BiomeMapManager");

        public int SaveInterval { get; set; } = 2500;

        public BiomeMapLevelConfig[] Levels { get; set; }

        static BiomeMapConfig()
        {
            Config = GetConfig();
        }

        private static BiomeMapConfig GetConfig()
        {
            try
            {
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiNetServer)).Location), "biomemap.json");
                if (!File.Exists(configPath))
                {
                    // Create config file
                    var newConfig = new BiomeMapConfig();
                    var newConfigJson = MiMapJsonConvert.SerializeObject(newConfig, true);
                    File.WriteAllText(configPath, newConfigJson);
                    Log.InfoFormat("Generating Config...");
                    return ProcessConfig(newConfig);
                }

                var json = File.ReadAllText(configPath);
                var config = MiMapJsonConvert.DeserializeObject<BiomeMapConfig>(json);

                Log.InfoFormat("Config Loaded from {0}", configPath);

                return ProcessConfig(config);
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Exception while loading BiomeMap Config:\n{0}", ex);
                throw;
            }
        }

        private static BiomeMapConfig ProcessConfig(BiomeMapConfig config)
        {
            if (!Path.IsPathRooted(config.TilesDirectory))
            {
                config.TilesDirectory =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiNetServer)).Location),
                        config.TilesDirectory);
            }

            Log.InfoFormat("TilesDirectory={0}", config.TilesDirectory);

            return config;
        }
    }
}
