using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BiomeMap.Drawing;
using BiomeMap.Runners;
using log4net;
using MiNET;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using Newtonsoft.Json;

namespace BiomeMap
{
    [Plugin(PluginName = "BiomeMapPlugin")]
    public class BiomeMapPlugin : Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapPlugin));

        public BiomeMapManager BiomeMapManager { get; }

        private readonly Dictionary<string, LevelRunner> _levelRunners = new Dictionary<string, LevelRunner>();

        public BiomeMapPlugin()
        {
            BiomeMapManager = new BiomeMapManager(GetConfig());
            //Log.InfoFormat("Config Loaded\n{0}", JsonConvert.SerializeObject(BiomeMapManager.Config, Formatting.Indented));
        }

        private BiomeMapConfig GetConfig()
        {
            try
            {
                var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiNetServer)).Location), "biomemap.json");
                if (!File.Exists(configPath))
                {
                    // Create config file
                    var newConfig = new BiomeMapConfig();
                    var newConfigJson = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                    File.WriteAllText(configPath, newConfigJson);
                    Log.InfoFormat("Generating Config...");
                    return newConfig;
                }

                var json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<BiomeMapConfig>(json);

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
            base.OnDisable();

        }
    }
}
