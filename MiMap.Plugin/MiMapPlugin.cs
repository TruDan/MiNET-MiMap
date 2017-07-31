using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using MiMap.Common;
using MiMap.Common.Configuration;
using MiMap.Drawing;
using MiMap.Plugin.Runners;
using MiMap.Web;
using MiNET.Plugins.Attributes;

namespace MiMap.Plugin
{
    [Plugin(PluginName = "MiMap.Plugin")]
    public class MiMapPlugin : MiNET.Plugins.Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapPlugin));

        public static MiMapConfig Config;

        public static MiMapPlugin Instance { get; private set; }

        public MiMapManager MiMapManager { get; }

        public LevelRunner[] LevelRunners => _levelRunners.Values.ToArray();

        private readonly Dictionary<string, LevelRunner> _levelRunners = new Dictionary<string, LevelRunner>();

        private MiMapWebServer _webServer;

        static MiMapPlugin()
        {
            Config = MiMapConfig.Config;
            if (!Directory.Exists(Config.TilesDirectory))
            {
                Directory.CreateDirectory(Config.TilesDirectory);
            }
        }

        public MiMapPlugin()
        {
            Instance = this;
            MiMapManager = new MiMapManager(Config);
            //Log.InfoFormat("Config Loaded\n{0}", JsonConvert.SerializeObject(MiMapManager.Config, Formatting.Indented));
            _webServer = new MiMapWebServer(Config.WebServer);
        }

        private void InitLevelRunners()
        {
            File.WriteAllText(Path.Combine(Config.TilesDirectory, "levels.json"), MiMapJsonConvert.SerializeObject(Config.Levels));

            foreach (var levelConfig in MiMapManager.Config.Levels)
            {
                var levelMap = MiMapManager.GetLevelMap(levelConfig.LevelId);
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

            if (Config.WebServer.Enabled)
            {
                _webServer.Start();
            }

            MiMapManager.Initialise();
            InitLevelRunners();
            MiMapManager.Start();

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

            MiMapManager.Stop();
            _levelRunners.Clear();

            if (Config.WebServer.Enabled)
            {
                _webServer.Stop();
            }

            base.OnDisable();

        }
    }
}
