using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BiomeMap.Drawing;
using BiomeMap.Plugin.Net;
using BiomeMap.Plugin.Runners;
using BiomeMap.Shared;
using BiomeMap.Shared.Configuration;
using log4net;
using MiNET;
using MiNET.Plugins.Attributes;
using Newtonsoft.Json;

namespace BiomeMap.Plugin
{
    [Plugin(PluginName = "BiomeMapPlugin")]
    public class BiomeMapPlugin : MiNET.Plugins.Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapPlugin));

        public static BiomeMapConfig Config { get; }

        public static BiomeMapPlugin Instance { get; private set; }

        public BiomeMapManager BiomeMapManager { get; }

        public LevelRunner[] LevelRunners => _levelRunners.Values.ToArray();

        private readonly Dictionary<string, LevelRunner> _levelRunners = new Dictionary<string, LevelRunner>();

        static BiomeMapPlugin()
        {
            Config = GetConfig();
        }

        public BiomeMapPlugin()
        {
            Instance = this;
            BiomeMapManager = new BiomeMapManager(Config);
            //Log.InfoFormat("Config Loaded\n{0}", JsonConvert.SerializeObject(BiomeMapManager.Config, Formatting.Indented));
        }

        private static BiomeMapConfig GetConfig()
        {
            try
            {
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiNetServer)).Location), "biomemap.json");
                if (!File.Exists(configPath))
                {
                    // Create config file
                    var newConfig = new BiomeMapPluginConfig();
                    var newConfigJson = MiMapJsonConvert.SerializeObject(newConfig, true);
                    File.WriteAllText(configPath, newConfigJson);
                    Log.InfoFormat("Generating Config...");
                    return newConfig;
                }

                var json = File.ReadAllText(configPath);
                var config = MiMapJsonConvert.DeserializeObject<BiomeMapPluginConfig>(json);

                Log.InfoFormat("Config Loaded from {0}", configPath);

                return config;
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Exception while loading BiomeMap Config:\n{0}", ex);
                throw;
            }
        }

        private void InitLevelRunners()
        {
            File.WriteAllText(Path.Combine(Config.TilesDirectory, "levels.json"), MiMapJsonConvert.SerializeObject(Config.Levels));

            foreach (var levelConfig in BiomeMapManager.Config.Levels)
            {
                var levelMap = BiomeMapManager.GetLevelMap(levelConfig.LevelId);
                if (levelMap != null)
                {
                    var runner = new LevelRunner(Context.Server, levelMap);
                    _levelRunners.Add(levelConfig.LevelId, runner);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            WsServer.Start();

            BiomeMapManager.Initialise();
            InitLevelRunners();
            BiomeMapManager.Start();

            foreach (var levelRunner in _levelRunners.Values.ToArray())
            {
                levelRunner.Start();
            }
        }

        public override void OnDisable()
        {
            foreach (var levelRunner in _levelRunners.Values.ToArray())
            {
                levelRunner.Stop();
            }

            BiomeMapManager.Stop();
            _levelRunners.Clear();
            WsServer.Stop();
            base.OnDisable();

        }
    }
}
