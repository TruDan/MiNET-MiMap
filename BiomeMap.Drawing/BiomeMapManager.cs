using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace BiomeMap.Drawing
{
    public class BiomeMapManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapManager));

        public BiomeMapConfig Config { get; }

        private readonly Dictionary<string, LevelMap> _levelMaps = new Dictionary<string, LevelMap>();

        public BiomeMapManager(BiomeMapConfig config)
        {
            Config = config;
        }

        public void Initialise()
        {
            foreach (var level in Config.Levels)
            {
                LoadLevel(level);
            }
            Log.InfoFormat("Biome Map Manager Initialised with {0} levels", _levelMaps.Count);
        }

        public LevelMap GetLevelMap(string levelId)
        {
            LevelMap levelMap;
            if (_levelMaps.TryGetValue(levelId, out levelMap))
            {
                return levelMap;
            }
            return null;
        }

        public void Start()
        {
            foreach (var levelMap in _levelMaps.Values.ToArray())
            {
                levelMap.Start();
            }
            Log.InfoFormat("Biome Map Manager Started");
        }

        public void Stop()
        {
            foreach (var levelMap in _levelMaps.Values.ToArray())
            {
                levelMap.Stop();
            }
            Log.InfoFormat("Biome Map Manager Stopped");
        }

        private void LoadLevel(BiomeMapLevelConfig config)
        {
            var levelMap = new LevelMap(this, config);
            _levelMaps.Add(config.LevelId, levelMap);
            Log.InfoFormat("Loaded Level {0}", config.LevelId);
        }
    }
}
