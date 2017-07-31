using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Drawing;
using log4net;
using MiMap.Common;
using MiMap.Common.Configuration;
using MiMap.Common.Data;
using MiMap.Drawing.Events;
using MiMap.Drawing.Layers;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing
{
    public class LevelMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LevelMap));

        public event EventHandler<TileUpdateEventArgs> OnTileUpdated;

        protected MiMapManager MiMapManager { get; }

        public LevelMeta Meta { get; }

        private string _metaPath { get; }

        public MiMapLevelConfig Config { get; }

        public string TilesDirectory { get; }
        private readonly object _layersSync = new object();
        private MapLayer[] Layers
        {
            get
            {
                lock (_layersSync)
                {
                    return _layers.ToArray();
                }
            }
        }

        private MapLayer[] _layers { get; } = { };

        private BlockBounds BlockBounds { get; set; }

        private readonly Dictionary<RegionPosition, LevelMapRegion> _regions = new Dictionary<RegionPosition, LevelMapRegion>();
        private readonly object _regionSync = new object();

        private Timer _timer;
        private readonly object _runSync = new object();

        public LevelMap(MiMapManager miMapManager, MiMapLevelConfig config)
        {
            MiMapManager = miMapManager;
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

            TilesDirectory = Path.Combine(MiMapManager.Config.TilesDirectory, config.LevelId);
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

            _layers = new MapLayer[config.Layers.Length + 1];
            var baseLayer = new MapLayer(this, new MiMapLevelLayerConfig()
            {
                LayerId = "base",
                BlendMode = BlendMode.Normal,
                Default = config.Enabled,
                Enabled = config.Enabled,
                Label = config.Label,
                Renderer = config.Renderer
            });

            baseLayer.OnTileUpdated += (s, e) => OnTileUpdated?.Invoke(s, e);
            var i = 0;
            _layers[i] = baseLayer;
            i++;

            foreach (var layer in config.Layers)
            {
                //Log.InfoFormat("Loading Overlay layer {0} {1}/{2}", layer, i, _layers.Length);
                var overlayerLayer = new MapLayer(this, layer);
                overlayerLayer.OnTileUpdated += (s, e) => OnTileUpdated?.Invoke(s, e);
                _layers[i] = overlayerLayer;
                i++;
            }

            _timer = new Timer(SaveLayers);
        }

        public LevelMapRegion GetRegionLayer(RegionPosition regionPos)
        {
            lock (_regionSync)
            {
                LevelMapRegion region;
                if (!_regions.TryGetValue(regionPos, out region))
                {
                    region = new LevelMapRegion(this, regionPos);
                    _regions.Add(regionPos, region);
                }

                return region;
            }
        }

        public void Start()
        {
            _timer.Change(0, MiMapManager.Config.SaveInterval);
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

            var region = GetRegionLayer(update.Position.GetRegionPosition());
            region.Update(update);

            var layers = Layers;
            if (layers.Length > 0)
            {
                foreach (var layer in layers)
                {
                    layer.UpdateBlockColumn(update);
                }
            }
        }

        private void UpdateMeta()
        {
            Meta.LastUpdate = DateTime.UtcNow;
            Meta.Size = new Size(BlockBounds.Width, BlockBounds.Height);
            Meta.Bounds = BlockBounds;

            var json = MiMapJsonConvert.SerializeObject(Meta);
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
                /*
                lock (_regionSync)
                {
                    foreach (var r in _regions.Values)
                    {
                        r.Save();
                    }
                }

                foreach (var l in Layers.ToArray())
                {
                    l.ProcessUpdate();
                }*/

                lock (_regionSync)
                {

                    //Parallel.ForEach(_regions.Values.ToArray(), region => region.Save());
                }
                var layers = Layers;
                if (layers.Length > 0)
                {
                    foreach (var l in layers)
                    {
                        //Log.InfoFormat("Updating layer {0}", l.LayerId);
                        l.ProcessUpdate();
                    }
                }
                //Parallel.ForEach(Layers, (layer) => layer.ProcessUpdate());

            }
            finally
            {
                Monitor.Exit(_runSync);
            }
        }
    }
}
