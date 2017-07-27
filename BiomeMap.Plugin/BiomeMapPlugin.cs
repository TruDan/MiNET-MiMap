using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BiomeMap.Drawing;
using BiomeMap.Plugin.Runners;
using BiomeMap.Common;
using BiomeMap.Common.Configuration;
using BiomeMap.Web;
using log4net;
using MiNET;
using MiNET.Plugins.Attributes;
using Newtonsoft.Json;

namespace BiomeMap.Plugin
{
    [Plugin(PluginName = "BiomeMap.Plugin")]
    public class BiomeMapPlugin : MiNET.Plugins.Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapPlugin));

        public static BiomeMapConfig Config;

        public static BiomeMapPlugin Instance { get; private set; }

        public BiomeMapManager BiomeMapManager { get; }

        public LevelRunner[] LevelRunners => _levelRunners.Values.ToArray();

        private readonly Dictionary<string, LevelRunner> _levelRunners = new Dictionary<string, LevelRunner>();

        private MiMapWebServer _webServer;

        static BiomeMapPlugin()
        {
            Config = BiomeMapConfig.Config;
            if (!Directory.Exists(Config.TilesDirectory))
            {
                Directory.CreateDirectory(Config.TilesDirectory);
            }
        }

        public BiomeMapPlugin()
        {
            Instance = this;
            BiomeMapManager = new BiomeMapManager(Config);
            //Log.InfoFormat("Config Loaded\n{0}", JsonConvert.SerializeObject(BiomeMapManager.Config, Formatting.Indented));
            _webServer = new MiMapWebServer();
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
            _webServer.Start();

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
            _webServer.Stop();
            base.OnDisable();

        }
    }
}
