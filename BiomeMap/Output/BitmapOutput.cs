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
using log4net;
using MiNET.Utils;

namespace BiomeMap.Output
{
    public class BitmapOutput : IRenderOutput
    {
        public const int TileSize = 256;
        public const int MaxZoom = 9;

        private static readonly ILog Log = LogManager.GetLogger(typeof(BitmapOutput));

        public static string OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RenderMaps");

        private Dictionary<ChunkCoordinates, Bitmap> _chunksCache = new Dictionary<ChunkCoordinates, Bitmap>();
        private List<ChunkCoordinates> _updatedRegions = new List<ChunkCoordinates>();

        public bool ReadOnly { get; set; } = false;

        public BitmapOutput()
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            var path = Path.Combine(OutputPath, "Raw");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            path = Path.Combine(OutputPath, "Region");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(OutputPath, "PlayerHead");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void WriteChunk(ChunkData data)
        {
            using (var chunkImg = GetChunkBitmap(data))
            {

                var path = Path.Combine(OutputPath, "Raw", data.X + "_" + data.Z + ".png");


                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                chunkImg.Save(path, ImageFormat.Png);

                var c = new ChunkCoordinates(data.X >> 5, data.Z >> 5);

                if(!_updatedRegions.Contains(c))
                    _updatedRegions.Add(c);
                /*for (int zoom = 0; zoom <= MaxZoom; zoom++)
                {
                    DrawZoomLevel(chunkImg, data.X, data.Z, zoom);
                }*/
            }

        }

        public void OnUpdateStart()
        {
            Log.InfoFormat("Starting update");
        }

        public void OnUpdateEnd()
        {
            var i = 0;

            // update caches
            foreach (var rC in _updatedRegions.ToArray())
            {
                DrawRegion(rC.X, rC.Z);
                i++;
            }
            _updatedRegions.Clear();
            Log.InfoFormat("Update Completed, Saved {0} regions", i);
        }

        private const int RegionSize = 32;

        private void DrawRegion(int regionX, int regionZ)
        {
            var xOffset = regionX * RegionSize;
            var zOffset = regionZ * RegionSize;

            using (var img = new Bitmap(16 * RegionSize, 16 * RegionSize))
            {
                using (var g = Graphics.FromImage(img))
                {
                    for (int x = 0; x < RegionSize; x++)
                    {
                        for (int z = 0; z < RegionSize; z++)
                        {
                            using (var chunkImg = GetChunkBitmap(xOffset + x, zOffset + z))
                            {
                                var p = new Point(x * 16, z * 16);
                                g.DrawImage(chunkImg, p);
                            }
                        }
                    }
                }

                var path = Path.Combine(OutputPath, "Region", "r." + regionX + "_" + regionZ + ".png");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                img.Save(path, ImageFormat.Png);
            }
        }

        private Bitmap GetChunkBitmap(int chunkX, int chunkZ)
        {
            var c = new ChunkCoordinates(chunkX, chunkZ);
            Bitmap img;

            if (!_chunksCache.TryGetValue(c, out img))
            {

                var path = Path.Combine(OutputPath, "Raw", chunkX + "_" + chunkZ + ".png");
                if (File.Exists(path))
                {
                    var chunkImg = Image.FromFile(path);
                    {
                        img = new Bitmap(chunkImg);
                    }
                }
                else
                {
                    img = new Bitmap(16, 16);
                    using (var g = Graphics.FromImage(img))
                    {
                        g.FillRectangle(Brushes.Black, 0, 0, 16, 16);
                    }
                }
                _chunksCache[c] = img;
            }

            return new Bitmap(img);
        }

        private void DrawZoomLevel(Bitmap chunkImg, int chunkX, int chunkZ, int zoomLevel)
        {
            var rX = (chunkX >> zoomLevel);
            var rZ = (chunkZ >> zoomLevel);

            var path = Path.Combine(OutputPath, zoomLevel.ToString(), rX + "_" + rZ + ".png");

            var dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Image img;

            try
            {
                using (var r = Image.FromFile(path))
                {
                    img = new Bitmap(r);
                }
            }
            catch (FileNotFoundException ex)
            {
                img = new Bitmap(256, 256);

                using (var g = Graphics.FromImage(img))
                {
                    g.Clear(Color.Black);
                }
            }

            using (img)
            {
                using (var g = Graphics.FromImage(img))
                {
                    g.Clear(Color.Black);
                    var size = (TileSize >> zoomLevel);

                    var x = (chunkX - (rX << zoomLevel)) * size;
                    var z = (chunkZ - (rZ << zoomLevel)) * size;

                    var rect = new Rectangle(
                        x < 0 ? TileSize - x - size : x,
                        z < 0 ? TileSize - z - size : z,
                        size,
                        size
                    );
                    Log.InfoFormat("Chunk@{8} {0},{1} => {2}_{3}.png @ ({4},{5}->{6},{7})",
                        chunkX,
                        chunkZ,
                        rX,
                        rZ,
                        rect.X,
                        rect.Y,
                        rect.Width,
                        rect.Height,
                        zoomLevel
                    );
                    g.DrawImage(chunkImg, rect);
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                img.Save(path);
            }
        }

        private Bitmap GetChunkBitmap(ChunkData data)
        {
            var img = new Bitmap(16, 16);


            using (var g = Graphics.FromImage(img))
            {
                g.Clear(Color.Black);
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        g.FillRectangle(new SolidBrush(data.BlockColors[x * 16 + z]), x, z, 1, 1);
                    }
                }

                //g.DrawString(data.X + "," + data.Z, SystemFonts.SmallCaptionFont, Brushes.White, 1, 1);
            }

            return img;
        }
    }
}
