using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Drawing.Renderers.Base;

namespace BiomeMap.Drawing.Layers
{
    public class BaseLayer : IMapLayer
    {
        public LevelMap Map { get; }

        public string Directory { get; }

        public ILayerRenderer Renderer { get; set; }

        private readonly ConcurrentDictionary<TilePosition, List<BlockColumnMeta>> _tileUpdates = new ConcurrentDictionary<TilePosition, List<BlockColumnMeta>>();
        
        public BaseLayer(LevelMap map) : this(map, Path.Combine(map.TilesDirectory, "base"), GetLayerRenderer(map.Config.BaseLayer))
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
            TilePosition[] toUpdate = _tileUpdates.Keys.ToArray();

            foreach (var tilePos in toUpdate)
            {
                List<BlockColumnMeta> updates;
                if (_tileUpdates.TryRemove(tilePos, out updates))
                {
                    using (var tile = new MapTile(this, tilePos))
                    {
                        foreach (var update in updates)
                        {
                            tile.Update(update);
                        }
                    }
                }
            }
        }

        public void UpdateBlockColumn(BlockColumnMeta column)
        {
            var tiles = GetTilePositionsForBlock(column.Position);

            foreach (var tilePos in tiles)
            {
                _tileUpdates.AddOrUpdate(tilePos, new List<BlockColumnMeta>(new[] { column }),
                    (position, list) =>
                    {
                        list.Add(column);
                        return list;
                    });
            }
        }

        private IEnumerable<TilePosition> GetTilePositionsForBlock(BlockPosition blockPos)
        {
            return new TilePosition[]{new TilePosition(blockPos.X >> 9, blockPos.Z >> 9, 0)};
            for (int zoom = Map.Meta.MinZoom; zoom <= Map.Meta.MaxZoom; zoom++)
            {


            }
        }

        private static ILayerRenderer GetLayerRenderer(BiomeMapLayerRenderer layerRenderer)
        {
            ILayerRenderer renderer = null;
            Console.WriteLine(layerRenderer);
            switch (layerRenderer)
            {
                default:
                case BiomeMapLayerRenderer.Default:
                    renderer = new DefaultLayerRenderer();
                    break;
                //case BiomeMapLayerRenderer.SolidColor:

                //    break;
                //case BiomeMapLayerRenderer.Texture:

                //    break;
            }

            return renderer;
        }
    }
}
