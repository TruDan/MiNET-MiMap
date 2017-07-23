using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Events;
using BiomeMap.Drawing.Layers;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Shared.Configuration;
using log4net;
using Newtonsoft.Json;
using Size = BiomeMap.Drawing.Data.Size;

namespace BiomeMap.Drawing
{
    public class LevelMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LevelMap));

        public event EventHandler<TileUpdateEventArgs> OnTileUpdated;

        protected BiomeMapManager BiomeMapManager { get; }

        public LevelMeta Meta { get; }

        private string _metaPath { get; }

        public BiomeMapLevelConfig Config { get; }

        public string TilesDirectory { get; }

        private IMapLayer[] Layers { get; }

        private BlockBounds BlockBounds { get; set; }

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
                Bounds = new BlockBounds(new BlockPosition(0, 0), new BlockPosition(0, 0)),
                LastUpdate = DateTime.MinValue
            };

            BlockBounds = new BlockBounds(new BlockPosition(0, 0), new BlockPosition(0, 0));

            TilesDirectory = Path.Combine(biomeMapManager.Config.TilesDirectory, config.LevelId);
            _metaPath = Path.Combine(TilesDirectory, "meta.json");

#if DEBUG
            if (Directory.Exists(TilesDirectory))
            {
                try
                {
                    Directory.Delete(TilesDirectory, true);
                }
                catch
                {

                }
            }
#endif

            Directory.CreateDirectory(TilesDirectory);

            var layers = new IMapLayer[config.Layers.Length + 1];
            var baseLayer = new BaseLayer(this, config.LevelId);
            baseLayer.OnTileUpdated += (s, e) => OnTileUpdated?.Invoke(s, e);
            layers[0] = baseLayer;

            var i = 1;
            foreach (var layer in config.Layers)
            {
                var overlayerLayer = new OverlayLayer(this, layer);
                overlayerLayer.OnTileUpdated += (s, e) => OnTileUpdated?.Invoke(s, e);
                layers[i] = overlayerLayer;
                i++;
            }

            Layers = layers;
            _timer = new Timer(SaveLayers);
        }

        public void Start()
        {
            _timer.Change(0, BiomeMapManager.Config.SaveInterval);
            Log.InfoFormat("Level Map Started: {0}", Meta.Id);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Log.InfoFormat("Level Map Stopped: {0}", Meta.Id);
        }

        public void UpdateBlockColumn(BlockColumnMeta update)
        {

            //Log.InfoFormat("{0}", chunkPos);
            BlockBounds.Min.X = Math.Min(update.Position.X, BlockBounds.Min.X);
            BlockBounds.Min.Z = Math.Min(update.Position.Z, BlockBounds.Min.Z);
            BlockBounds.Max.X = Math.Max(update.Position.X, BlockBounds.Max.X);
            BlockBounds.Max.Z = Math.Max(update.Position.Z, BlockBounds.Max.Z);

            foreach (var layer in Layers)
            {
                layer.UpdateBlockColumn(update);
            }
        }

        private void UpdateMeta()
        {
            Meta.LastUpdate = DateTime.UtcNow;
            Meta.Size = new Size(BlockBounds.Width, BlockBounds.Height);
            Meta.Bounds = BlockBounds;

            var json = JsonConvert.SerializeObject(Meta);
            File.WriteAllText(_metaPath, json);
        }

        private void UpdateBiomePolygons()
        {

        }

        private void SaveLayers(object state)
        {
            if (!Monitor.TryEnter(_runSync))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(TilesDirectory);

                UpdateMeta();

                Parallel.ForEach(Layers, (layer) => layer.ProcessUpdate());
            }
            finally
            {
                Monitor.Exit(_runSync);
            }
        }
    }
}
