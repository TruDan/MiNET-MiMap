using System.Collections.Generic;
using System.Linq;
using log4net;
using MiMap.Common.Configuration;

namespace MiMap.Drawing
{
    public class MiMapManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapManager));

        public MiMapConfig Config { get; }

        private readonly Dictionary<string, LevelMap> _levelMaps = new Dictionary<string, LevelMap>();

        public MiMapManager(MiMapConfig config)
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

        private void LoadLevel(MiMapLevelConfig config)
        {
            var levelMap = new LevelMap(this, config);
            _levelMaps.Add(config.LevelId, levelMap);
            Log.InfoFormat("Loaded Level {0}", config.LevelId);
        }
    }
}
