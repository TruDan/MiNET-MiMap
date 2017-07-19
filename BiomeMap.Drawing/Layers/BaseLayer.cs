using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Drawing.Renderers.Base;
using BiomeMap.Drawing.Renderers.Overlay;
using BiomeMap.Drawing.Renderers.PostProcessors;
using BiomeMap.Shared.Configuration;
using log4net;
using Size = BiomeMap.Drawing.Data.Size;

namespace BiomeMap.Drawing.Layers
{
    public class BaseLayer : IMapLayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BaseLayer));

        private const int MaxUpdatesPerInterval = int.MaxValue;

        public LevelMap Map { get; }

        public string Directory { get; }

        public BlendMode BlendMode { get; protected set; }

        public ILayerRenderer Renderer { get; set; }

        public IPostProcessor[] PostProcessors { get; protected set; } = { new LightingPostProcessor(), new HeightShadowPostProcessor() };

        private readonly ConcurrentQueue<BlockColumnMeta> _updates = new ConcurrentQueue<BlockColumnMeta>();

        private readonly ConcurrentDictionary<TilePosition, List<BlockColumnMeta>> _tileUpdates =
            new ConcurrentDictionary<TilePosition, List<BlockColumnMeta>>(new TilePositionComparer());

        public BaseLayer(LevelMap map) : this(map, Path.Combine(map.TilesDirectory, "base"),
            GetLayerRenderer(map.Config.BaseLayer))
        {

        }

        protected BaseLayer(LevelMap map, string directory, ILayerRenderer renderer)
        {
            Map = map;
            Directory = directory;
            Renderer = renderer;
        }

        public void ProcessUpdate()
        {
            var i = 0;

            var updates = new List<BlockColumnMeta>();

            BlockColumnMeta update;
            while (i < MaxUpdatesPerInterval && _updates.TryDequeue(out update))
            {
                updates.Add(update);
                i++;
            }

            if (updates.Count == 0) return;

            var tileCount = 0;
            for (var z = Map.Meta.MinZoom; z <= Map.Meta.MaxZoom; z++)
            {
                tileCount += (int)Math.Pow(1 << z, 2);
            }

            var regions = updates.GroupBy(c => c.Position.GetRegionPosition());
            //Parallel.ForEach(regions, (r) =>

            foreach (var r in regions)
            {
                using (var region = new MapRegionLayer(this, r.Key))
                {
                    var j = 0;
                    var sw = Stopwatch.StartNew();

                    foreach (var u in r.ToArray())
                    {
                        region.Update(u);
                        j++;
                    }

                    Log.InfoFormat("Saving Region {0} with {1} updates in {2}ms ({3} tiles)", r.Key, j, sw.ElapsedMilliseconds, tileCount);
                }
            };

        }



        public void ProcessUpdateOld()
        {
            foreach (var tilePos in _tileUpdates.Keys.ToArray())
            {
                List<BlockColumnMeta> updates;
                if (_tileUpdates.TryRemove(tilePos, out updates))
                {
                    using (var tile = new MapTile(this, tilePos))
                    {
                        foreach (var update in updates.ToArray())
                        {
                            tile.Update(update);

                        }
                    }
                }
            };
        }

        public void UpdateBlockColumn(BlockColumnMeta column)
        {
            _updates.Enqueue(column);
        }

        public void UpdateBlockColumnOld(BlockColumnMeta column)
        {
            var tiles = GetTilePositionsForBlock(column.Position);

            foreach (var tilePos in tiles)
            {
                _tileUpdates.AddOrUpdate(tilePos, (p) =>
                {
                    //Debug.WriteLine("NEW positions for {0}: {1}", column.Position, tilePos);
                    return new List<BlockColumnMeta>(new[] { column });
                },
                    (position, list) =>
                    {
                        if (list.All(m => !Equals(m.Position, column.Position)))
                        {
                            list.Add(column);
                            //Debug.WriteLine("Position for {0}: {1}", column.Position, tilePos);
                        }
                        return list;
                    });
            }
        }

        private IEnumerable<TilePosition> GetTilePositionsForBlock(BlockPosition blockPos)
        {
            //return new TilePosition[] { new TilePosition(blockPos.X >> 9, blockPos.Z >> 9, 0) };
            for (int zoom = Map.Meta.MinZoom; zoom <= Map.Meta.MaxZoom; zoom++)
            {
                yield return new TilePosition(blockPos.X >> (9 - zoom), blockPos.Z >> (9 - zoom), zoom);
            }
        }

        private static ILayerRenderer GetLayerRenderer(BiomeMapLayerRenderer layerRenderer)
        {
            ILayerRenderer renderer = null;
            switch (layerRenderer)
            {
                default:
                case BiomeMapLayerRenderer.Default:
                    renderer = new DefaultLayerRenderer();
                    break;
                //case BiomeMapLayerRenderer.SolidColor:

                //    break;
                case BiomeMapLayerRenderer.Texture:
                    renderer = new TextureLayerRenderer();
                    break;
            }

            return renderer;
        }
    }
}
