using System;
using System.IO;
using System.Reflection;
using log4net;
using MiNET;
using Newtonsoft.Json;

namespace MiMap.Common.Configuration
{
    public class MiMapConfig
    {

        #region Static
        [JsonIgnore]
        public static MiMapConfig Config { get; }

        [JsonIgnore]
        public static string BasePath { get; } =
            Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiNetServer)).Location), "MiMap");

        [JsonIgnore]
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapConfig));

        private static MiMapConfig GetConfig()
        {
            try
            {
                Directory.CreateDirectory(BasePath);

                var configPath = Path.Combine(BasePath, "config.json");
                if (!File.Exists(configPath))
                {
                    // Create config file
                    var newConfig = new MiMapConfig();
                    var newConfigJson = MiMapJsonConvert.SerializeObject(newConfig, true);
                    File.WriteAllText(configPath, newConfigJson);
                    Log.InfoFormat("Generating Config...");
                    return ProcessConfig(newConfig);
                }

                var json = File.ReadAllText(configPath);
                var config = MiMapJsonConvert.DeserializeObject<MiMapConfig>(json);

                Log.InfoFormat("Config Loaded from {0}", configPath);

                return ProcessConfig(config);
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Exception while loading MiMap Config:\n{0}", ex);
                throw;
            }
        }

        private static MiMapConfig ProcessConfig(MiMapConfig config)
        {
            if (!Path.IsPathRooted(config.TilesDirectory))
            {
                config.TilesDirectory =
                    Path.Combine(BasePath,
                        config.TilesDirectory);
            }

            Log.InfoFormat("TilesDirectory={0}", config.TilesDirectory);

            return config;
        }

        #endregion

        public string TilesDirectory { get; set; } = "tiles";

        public int SaveInterval { get; set; } = 2500;

        public MiMapWebServerConfig WebServer { get; set; } = new MiMapWebServerConfig();

        public MiMapLevelConfig[] Levels { get; set; } = new MiMapLevelConfig[0];

        static MiMapConfig()
        {
            Config = GetConfig();
        }

    }
}
