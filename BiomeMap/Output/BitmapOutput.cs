using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Data;
using BiomeMap.Drawing;
using log4net;
using Microsoft.IO;
using MiNET.Utils;

namespace BiomeMap.Output
{
    public partial class BitmapOutput : IRenderOutput
    {

        protected BiomeMapLevelHandler Handler { get; }

        public const int RegionSize = 16;

        public const int TileSize = RegionSize * 16;

        public const int MaxZoom = 3;

        private static readonly Pen GridPen = new Pen(Color.FromArgb(50, Color.Gray), 1);

        private static readonly ILog Log = LogManager.GetLogger(typeof(BitmapOutput));

        public static string BasePath = Config.GetProperty("BiomeMapWebRoot", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        public static string TilesPath = Path.Combine(BasePath, "tiles");
        public static string PlayersPath = Path.Combine(BasePath, "players");

        private readonly Dictionary<int, Dictionary<Point, TileBitmap>> _tileCache = new Dictionary<int, Dictionary<Point, TileBitmap>>();
        
        private readonly Dictionary<Point, TileRegion> _regions = new Dictionary<Point, TileRegion>();

        public bool ReadOnly { get; set; } = false;

        public BitmapOutput(BiomeMapLevelHandler handler)
        {
            Handler = handler;

            if (Directory.Exists(TilesPath))
            {
                Directory.Delete(TilesPath, true);
            }

            Directory.CreateDirectory(TilesPath);

            /*for (int i = 0; i <= MaxZoom; i++)
            {
                var p = Path.Combine(TilesPath, i.ToString());
                Directory.CreateDirectory(p);
            }*/
            
            Directory.CreateDirectory(PlayersPath);
        }

        private TileRegion GetRegion(int regionX, int regionZ)
        {
            TileRegion region;
            if (!_regions.TryGetValue(new Point(regionX, regionZ), out region))
            {
                region = new TileRegion(Handler.Level.LevelId, regionX, regionZ);
                _regions.Add(new Point(regionX, regionZ), region);
            }
            return region;
        }

        public void WriteChunk(ChunkData data)
        {
            var region = GetRegion(data.X >> 5, data.Z >> 5);
            region.DrawChunk(data);

            /*
            var tiles = GetTiles(data.X, data.Z);
            foreach (var tile in tiles)
            {
                var offsetX = (data.X % tile.ChunkBounds.Width) * 16;
                var offsetZ = (data.Z % tile.ChunkBounds.Height) * 16;
                var scaleX = TileSize / (tile.ChunkBounds.Width * 16f);
                var scaleY = TileSize / (tile.ChunkBounds.Width * 16f);

                using (var g = tile.CreateGraphics())
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            g.FillRectangle(new SolidBrush(data.BlockColors[x * 16 + z]), offsetX + x, offsetZ + z, scaleX, scaleY);
                        }
                    }
                }
            }
            */
        }

        private TileBitmap[] GetTiles(int chunkX, int chunkZ)
        {
            //Log.InfoFormat("Getting Tiles for Chunk {0},{1}", chunkX, chunkZ);

            var rC = new Point(chunkX, chunkZ);

            var tiles = new List<TileBitmap>();

            for (int i = 0; i <= MaxZoom; i++)
            {
                var p = GetTileCoordinates(rC, i);
                
                tiles.Add(GetTile(p, i));
            }

            return tiles.ToArray();
        }

        private Point GetTileCoordinates(Point chunkCoords, int zoomLevel)
        {
            /*
             * TileX            = (ChunkX / RegionSizeX) << Zoom
             * TileX >> Zoom    = ChunkX / RegionSizeX
             * (TileX >> Zoom) * RegionSizeX = ChunkX
             */

            var tiles = RegionSize >> zoomLevel;
            var tX = chunkCoords.X % tiles;
            var tY = chunkCoords.Y % tiles;

            return new Point(tX, tY);
        }

        private TileBitmap GetTile(Point coords, int zoom)
        {
            Dictionary<Point, TileBitmap> zoomDict;
            if (!_tileCache.TryGetValue(zoom, out zoomDict))
            {
                zoomDict = new Dictionary<Point, TileBitmap>();
                _tileCache.Add(zoom, zoomDict);
            }

            TileBitmap tile;
            if (!zoomDict.TryGetValue(coords, out tile))
            {
                tile = new TileBitmap(new Bitmap(TileSize, TileSize), "Overworld", coords.X, coords.Y, zoom);
                zoomDict.Add(coords, tile);
            }

            return tile;
        }

        public void OnUpdateStart()
        {
            Log.InfoFormat("Starting update");
        }

        public void OnUpdateEnd()
        {
            var i = 0;
            
            // update caches
            /*foreach (var tiles in _tileCache.Values.ToArray())
            {
                foreach (var tile in tiles.Values.ToArray())
                {
                    if(tile.Save())
                        i++;
                }
            }*/

            foreach (var region in _regions.Values.ToArray())
            {
                i += region.Save();
            }
            
            Log.InfoFormat("Update Completed, Saved {0} tiles", i);
        }
        

        private static Font DebugFont = new Font(FontFamily.GenericMonospace, 12, GraphicsUnit.Pixel);

        private void DrawRegionZoom(int regionX, int regionZ, Bitmap regionBase, int zoomLevel)
        {
            
            
            var xOffset = regionX << zoomLevel;
            var zOffset = regionZ << zoomLevel;

            //xOffset -= pow / (regionX < 0 ? -2 : 2);
            //zOffset -= pow / (regionZ < 0 ? -2 : 2);

            var tiles = 1 << zoomLevel;
            var size = TileSize / tiles;

            for (int x = 0; x < tiles; x++)
            {
                for (int z = 0; z < tiles; z++)
                {
                    using (var img = new Bitmap(TileSize, TileSize))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            //g.CompositingQuality = CompositingQuality.AssumeLinear;
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            //g.SmoothingMode = SmoothingMode.None;
                            //g.PageUnit = GraphicsUnit.Pixel;

                            //g.DrawImage(
                            //   new Bitmap(regionBase), new Rectangle(0, 0, img.Width, img.Height), 0f, 0f, (float)size, (float)size, GraphicsUnit.Pixel);

                            var src = new Rectangle(x*size, z*size, size, size);

                            g.DrawImage(regionBase, new Rectangle(0, 0, TileSize, TileSize), src, GraphicsUnit.Pixel);

                           // var debugText = (xOffset + x) + ", " + (zOffset +z) + " @ " + zoomLevel + "\nR:" + regionX + "," + regionZ + "(" + x + "," + z + ")\n" + src.ToString();

                            //var s = g.MeasureString(debugText, DebugFont);

                            //g.DrawString(debugText, DebugFont, Brushes.Black, (img.Width - s.Width) / 2f + 3, (img.Height - s.Height) / 2f + 3);
                            //g.DrawString(debugText, DebugFont, Brushes.WhiteSmoke, (img.Width-s.Width)/2f, (img.Height-s.Height)/2f);

                            g.Flush(FlushIntention.Flush);
                        }
                        
                       // img.SaveTile(xOffset + x, zOffset + z, zoomLevel);
                    }
                }
            }
            
        }
    }
}
