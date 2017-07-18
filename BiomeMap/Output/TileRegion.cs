using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Data;

namespace BiomeMap.Output
{
    public class TileRegion
    {
        public const int ChunksPerRegion = 32;

        public const int BlocksPerChunk = 16;

        public const int BlocksPerRegion = ChunksPerRegion * BlocksPerChunk;

        public string LevelName { get; }

        public int X { get; }
        public int Z { get; }

        public Rectangle Bounds { get; }

        private Dictionary<int, TileCollection> _tiles = new Dictionary<int, TileCollection>();
        public bool IsDirty { get; private set; }

        public TileRegion(string levelName, int x, int z)
        {
            LevelName = levelName;
            X = x;
            Z = z;

            Bounds = new Rectangle(X * BlocksPerRegion, Z * BlocksPerRegion, BlocksPerRegion, BlocksPerRegion);

            for (int i = 0; i <= 3; i++)
            {
                GetTileCollection(i);
            }
        }
        public int Save()
        {
            if (!IsDirty) return 0;
            IsDirty = false;

            var i = 0;
            foreach (var tile in _tiles.Values.ToArray())
            {
                i += tile.Save();
            }
            return i;
        }

        public void DrawChunk(ChunkData data)
        {
            foreach (var collection in _tiles.Values.ToArray())
            {
                collection.DrawChunk(data);
            }
        }

        private TileCollection GetTileCollection(int zoomLevel)
        {
            TileCollection tiles;
            if (!_tiles.TryGetValue(zoomLevel, out tiles))
            {
                tiles = new TileCollection(this, zoomLevel);
                _tiles.Add(zoomLevel, tiles);
            }
            return tiles;
        }

        internal void Dirty()
        {
            IsDirty = true;
        }

        class TileCollection
        {
            internal TileRegion Region { get; }

            public int Zoom { get; }

            public int GridSize { get; }

            public int BlocksPerTile { get; }
            
            private Dictionary<Point, Tile> _tiles = new Dictionary<Point, Tile>();
            public bool IsDirty { get; private set; }

            public TileCollection(TileRegion region, int zoom)
            {
                Region = region;
                Zoom = zoom;
                GridSize = (1 << zoom)*2;

                BlocksPerTile = (int)Math.Ceiling((double)Region.Bounds.Width / GridSize);
            }

            public int Save()
            {
                if (!IsDirty) return 0;
                IsDirty = false;

                var i = 0;
                foreach (var tile in _tiles.Values.ToArray())
                {
                    if (tile.Save())
                        i++;
                }
                return i;
            }

            public void DrawChunk(ChunkData data)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        var tile = GetTileForBlock(data.X * 16 + x, data.Z * 16 + z);
                        tile.SetBlockColor(data.X * 16 + x, data.Z * 16 + z, data.BlockColors[x * 16 + z]);
                    }
                }

            }

            private Tile GetTile(int tileX, int tileY)
            {
                Tile tile;
                if (!_tiles.TryGetValue(new Point(tileX, tileY), out tile))
                {
                    tile = new Tile(this, tileX, tileY);
                    _tiles.Add(new Point(tileX, tileY), tile);
                }
                return tile;
            }

            private Tile GetTileForBlock(int blockX, int blockZ)
            {
                var tileX = blockX / (BlocksPerTile * GridSize);
                var tileY = blockZ / (BlocksPerTile * GridSize);
                if (blockX < 0)
                {
                    tileX--;
                }
                if (blockZ < 0)
                {
                    tileY--;
                }
                
                return GetTile(tileX, tileY);
            }

            internal void Dirty()
            {
                IsDirty = true;
                Region.Dirty();
            }
        }

        class Tile
        {
            public const int TileSize = 256;

            protected TileCollection Collection { get; }

            protected string FilePath { get; }

            public int X { get; }
            public int Y { get; }

            protected int BlockOffsetX { get; }
            protected int BlockOffsetY { get; }
            protected int BlockSize { get; }

            private Bitmap _bitmap { get; }

            public bool IsDirty { get; private set; }
            
            public Tile(TileCollection collection, int x, int y)
            {
                Collection = collection;
                X = x;
                Y = y;
                _bitmap = new Bitmap(TileSize, TileSize);

                BlockSize = (int)Math.Round((double)TileSize / (Collection.BlocksPerTile * BlockSize));
                
                BlockOffsetX = x * (Collection.BlocksPerTile * BlockSize);
                BlockOffsetY = y * Collection.BlocksPerTile * BlockSize;

                FilePath = Path.Combine(BitmapOutput.TilesPath, Collection.Region.LevelName, Collection.Zoom.ToString(), X + "_" + Y + ".png");
            }

            public void SetBlockColor(int blockX, int blockZ, Color color)
            {
                IsDirty = true;
                Collection.Dirty();

                var x = blockX - BlockOffsetX;
                var y = blockZ - BlockOffsetY;

                for (int i = 0; i < BlockSize; i++)
                {
                    for (int j = 0; j < BlockSize; j++)
                    {
                        _bitmap.SetPixel(x + i, y + j, color);
                    }
                }
            }

            public bool Save()
            {
                if (!IsDirty) return false;
                IsDirty = false;

                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                _bitmap.Save(FilePath, ImageFormat.Png);
                return true;
            }
        }
    }
}
