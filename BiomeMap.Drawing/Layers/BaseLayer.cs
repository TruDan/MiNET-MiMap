using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Drawing.Renderers.Base;
using BiomeMap.Shared;
using BiomeMap.Shared.Data;

namespace BiomeMap.Drawing.Layers
{
    public class BaseLayer : IMapLayer
    {
        public LevelMap Map { get; }

        public string Directory { get; }

        public BlendMode BlendMode { get; protected set; }

        public ILayerRenderer Renderer { get; set; }

        public Color Background { get; protected set; }

        private readonly ConcurrentDictionary<TilePosition, List<BlockColumnMeta>> _tileUpdates = new ConcurrentDictionary<TilePosition, List<BlockColumnMeta>>(new TilePositionComparer());

        public BaseLayer(LevelMap map) : this(map, Path.Combine(map.TilesDirectory, "base"), GetLayerRenderer(map.Config.BaseLayer))
        {
            Background = Color.FromArgb(0x7F121212);
        }

        protected BaseLayer(LevelMap map, string directory, ILayerRenderer renderer)
        {
            Map = map;
            Directory = directory;
            Renderer = renderer;
        }

        public void ProcessUpdate()
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
            //            return new TilePosition[] { new TilePosition(blockPos.X >> 9, blockPos.Z >> 9, 0) };
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
                    //case BiomeMapLayerRenderer.Texture:

                    //    break;
            }

            return renderer;
        }
    }
}
