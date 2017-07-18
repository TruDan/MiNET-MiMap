using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;
using BiomeMap.Drawing.Renderers;
using log4net;
using Newtonsoft.Json;

namespace BiomeMap.Drawing
{
    public class LevelMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LevelMap));

        protected BiomeMapManager BiomeMapManager { get; }

        public LevelMeta Meta { get; }

        private string _metaPath { get; }

        public BiomeMapLevelConfig Config { get; }

        public string TilesDirectory { get; }

        private IMapLayer[] Layers { get; }

        private ChunkBounds ChunkBounds { get; set; }

        private Timer _timer;
        private readonly object _runSync = new object();

        public LevelMap(BiomeMapManager biomeMapManager, BiomeMapLevelConfig config)
        {
            BiomeMapManager = biomeMapManager;
            Config = config;
            Meta = new LevelMeta()
            {
                Id = config.LevelId,
                Name = string.IsNullOrEmpty(config.Label) ? config.LevelId : config.Label,
                MinZoom = config.MinZoom,
                MaxZoom = config.MaxZoom,
                TileSize = new Size(config.TileSize, config.TileSize),
                Size = new Size(),
                Bounds = new ChunkBounds(new ChunkPosition(0,0), new ChunkPosition(0,0)),
                LastUpdate = DateTime.MinValue
            };

            ChunkBounds = new ChunkBounds(new ChunkPosition(0,0), new ChunkPosition(0,0));

            TilesDirectory = Path.Combine(biomeMapManager.Config.TilesDirectory, config.LevelId);
            _metaPath = Path.Combine(TilesDirectory, "meta.json");

            Directory.CreateDirectory(TilesDirectory);

            var layers = new IMapLayer[config.Layers.Length+1];
            layers[0] = new BaseLayer(this);

            var i = 1;
            foreach (var layer in config.Layers)
            {
                layers[i] = new OverlayLayer(this, layer);
                i++;
            }

            Layers = layers;
            _timer = new Timer(SaveLayers);
        }

        public void Start()
        {
            _timer.Change(BiomeMapManager.Config.SaveInterval, BiomeMapManager.Config.SaveInterval);
            Log.InfoFormat("Level Map Started: {0}", Meta.Id);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Log.InfoFormat("Level Map Stopped: {0}", Meta.Id);
        }

        public void UpdateBlockColumn(BlockColumnMeta update)
        {

            var chunkPos = update.Position.GetChunkPosition();
            //Log.InfoFormat("{0}", chunkPos);
            ChunkBounds.Min.X = Math.Min(chunkPos.X, ChunkBounds.Min.X);
            ChunkBounds.Min.Z = Math.Min(chunkPos.Z, ChunkBounds.Min.Z);
            ChunkBounds.Max.X = Math.Max(chunkPos.X, ChunkBounds.Max.X);
            ChunkBounds.Max.Z = Math.Max(chunkPos.Z, ChunkBounds.Max.Z);

            foreach (var layer in Layers)
            {
                layer.UpdateBlockColumn(update);
            }
        }

        private void UpdateMeta()
        {
            Meta.LastUpdate = DateTime.UtcNow;
            Meta.Size = new Size(ChunkBounds.Width, ChunkBounds.Height);
            Meta.Bounds = ChunkBounds;

            var json = JsonConvert.SerializeObject(Meta);
            File.WriteAllText(_metaPath, json);
        }

        private void SaveLayers(object state)
        {
            if (!Monitor.TryEnter(_runSync))
            {
                return;
            }

            try
            {
                UpdateMeta();

                foreach (var layer in Layers)
                {
                    layer.ProcessUpdate();
                }
            }
            finally
            {
                Monitor.Exit(_runSync);
            }
        }
    }
}
