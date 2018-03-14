using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using MiMap.Common;
using MiMap.Common.Configuration;
using MiMap.Common.Data;
using MiMap.Drawing.Events;
using MiMap.Drawing.Renderers;
using MiMap.Drawing.Renderers.Base;
using MiMap.Drawing.Renderers.PostProcessors;

namespace MiMap.Drawing.Layers
{
    public class MapLayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MapLayer));

        private const int MaxUpdatesPerInterval = (1 << 9) * (1 << 9);
        private static readonly TimeSpan CleanupTimespan = TimeSpan.FromSeconds(30);

        public event EventHandler<TileUpdateEventArgs> OnTileUpdated;

        public string LayerId { get; }

        public string Directory { get; }

        public LevelMap Map { get; }

        public ILayerRenderer Renderer { get; set; }

        public IPostProcessor[] PostProcessors { get; protected set; }

        private readonly Dictionary<RegionPosition, MapRegionLayer> _regions = new Dictionary<RegionPosition, MapRegionLayer>();
        private readonly object _cleanupSync = new object();

        private readonly ConcurrentQueue<BlockColumnMeta> _updates = new ConcurrentQueue<BlockColumnMeta>();

        private TileScaler Scaler { get; }

        private Timer CleanupTimer { get; }

        public MapLayer(LevelMap map, MiMapLevelLayerConfig config)
        {
            Map = map;
            LayerId = config.LayerId ?? "base";
            Directory = Path.Combine(map.TilesDirectory, LayerId);
            Renderer = RendererFactory.CreateLayerRenderer(config.Renderer);

            var postProcessors = new List<IPostProcessor>();
            foreach (var pp in config.Renderer.PostProcessors)
            {
                var p = PostProcessorFactory.CreatePostProcessor(pp);
                if (p != null)
                {
                    postProcessors.Add(p);
                }
            }
            PostProcessors = postProcessors.ToArray();

            Scaler = new TileScaler(Directory, Renderer.RenderScale, map.Meta.TileSize, map.Meta.MinZoom, map.Meta.MaxZoom, map.Config.LevelId, LayerId);
            Scaler.OnTileUpdated += (s, e) => OnTileUpdated?.Invoke(s, e);
            //CleanupTimer = new Timer(CleanupCallback, null, 5000, 5000);
        }

        private MapRegionLayer GetRegionLayer(RegionPosition regionPos)
        {
            lock (_cleanupSync)
            {
                MapRegionLayer layer;
                if (!_regions.TryGetValue(regionPos, out layer))
                {
                    layer = new MapRegionLayer(this, regionPos);
                    _regions.Add(regionPos, layer);
                }

                return layer;
            }
        }

        private void CleanupCallback(object state)
        {
            lock (_cleanupSync)
            {
                foreach (var layer in _regions.Values.ToArray())
                {
                    layer.Save();
                    if ((DateTime.UtcNow - layer.LastUpdated) > CleanupTimespan)
                    {
                        _regions.Remove(layer.Position);
                        layer.Dispose();
                    }
                }
            }
        }

        public void ProcessUpdate()
        {
            var updateSw = Stopwatch.StartNew();
            var i = 0;

            var updates = new List<BlockColumnMeta>();

            BlockColumnMeta update;
            while (i < MaxUpdatesPerInterval && _updates.TryDequeue(out update))
            {
                updates.Add(update);
                i++;
            }

            if (updates.Count == 0) return;

            var regions = updates.GroupBy(c => c.Position.GetRegionPosition());
            //Parallel.ForEach(regions, (r) =>

            var k = 0;
            foreach (var r in regions)
            {
                var region = GetRegionLayer(r.Key);

                var j = 0;
                var sw = Stopwatch.StartNew();

                foreach (var u in r.ToArray())
                {
                    region.Update(u);
                    j++;
                    k++;
                }

                Scaler.Enqueue(r.Key, region);
                region.ClearUpdatedBlocks();
                Log.InfoFormat("Saving Region {0} with {1} updates in {2}ms", r.Key, j, sw.ElapsedMilliseconds);
            }

            if (k > 0)
                //if (updateSw.ElapsedMilliseconds > 500)
                Log.InfoFormat("Layer {0} updated in {1}ms ({2} updates)", GetType().Name, updateSw.ElapsedMilliseconds, k);
        }


        public void UpdateBlockColumn(BlockColumnMeta column)
        {
            _updates.Enqueue(MiMapJsonConvert.DeserializeObject<BlockColumnMeta>(MiMapJsonConvert.SerializeObject(column)));
        }
    }
}
