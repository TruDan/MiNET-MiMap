using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;
using BiomeMap.Drawing.Utils;
using log4net;
using Newtonsoft.Json;

namespace BiomeMap.Drawing
{
    public class MapRegionLayer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MapRegionLayer));


        public IMapLayer Layer { get; }

        public RegionPosition Position { get; }

        public Dictionary<BlockPosition, BlockColumnMeta> Blocks { get; private set; } = new Dictionary<BlockPosition, BlockColumnMeta>();

        public DateTime LastUpdated { get; private set; }

        private string FilePath { get; }

        private Bitmap Bitmap { get; set; }
        private readonly object _bitmapSync = new object();

        private RectangleF BitmapBounds { get; set; }

        private Graphics Graphics { get; set; }

        private bool IsNew { get; set; }

        public MapRegionLayer(IMapLayer layer, RegionPosition pos)
        {
            Layer = layer;
            Position = pos;
            FilePath = Path.Combine(layer.Directory, ".regions",
                $"{Position.X}_{Position.Z}.png");
            Load();
        }

        private void Load()
        {
            //Log.InfoFormat("Loading Tile {0},{1}@{2}", Position.X, Position.Z, Position.Zoom);
            Bitmap bitmap;
            if (File.Exists(FilePath))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var img = Image.FromStream(fs))
                    {
                        bitmap = new Bitmap(img);
                    }
                }
            }
            else
            {
                bitmap = new Bitmap(Layer.Renderer.RenderScale.Width * (1 << 9), Layer.Renderer.RenderScale.Height * (1 << 9));
                IsNew = true;
            }

            var metaPath = Path.Combine(Path.GetDirectoryName(FilePath),
                Path.GetFileNameWithoutExtension(FilePath) + ".json");
            if (File.Exists(metaPath))
            {
                using (var fs = new FileStream(metaPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var gs = new GZipStream(fs, CompressionLevel.Optimal))
                    {
                        using (var sr = new StreamReader(gs))
                        {
                            var json = sr.ReadToEnd();
                            Blocks = JsonConvert.DeserializeObject<Dictionary<BlockPosition, BlockColumnMeta>>(json);
                        }
                    }
                }
            }

            Bitmap = bitmap;
            BitmapBounds = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            Graphics = bitmap.GetGraphics();

            if (IsNew)
            {
                Graphics.Clear(Layer.Renderer.Background);
            }
        }

        public void Save()
        {
            //PostProcess();

            //Parallel.For(Layer.Map.Meta.MinZoom, Layer.Map.Meta.MaxZoom + 1, SplitRegionForZoom);

            lock (_bitmapSync)
            {
                Bitmap.SaveToFile(FilePath);
            }

            var metaPath = Path.Combine(Path.GetDirectoryName(FilePath),
                Path.GetFileNameWithoutExtension(FilePath) + ".json");
            using (var fs = new FileStream(metaPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var gs = new GZipStream(fs, CompressionLevel.Optimal))
                {
                    using (var sr = new StreamWriter(gs))
                    {
                        var json = JsonConvert.SerializeObject(Blocks);
                        sr.Write(json);
                    }
                }
            }

        }

        private void SplitRegionForZoom(int zoomLevel)
        {
            var w = 1 << zoomLevel;

            Bitmap rImg;
            lock (_bitmapSync)
            {
                rImg = Bitmap.Clone(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), Bitmap.PixelFormat);
            }

            using (rImg)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < w; y++)
                    {
                        var tileX = (Position.X << zoomLevel) + x;
                        var tileY = (Position.Z << zoomLevel) + y;
                        var tilePath = Path.Combine(Layer.Directory, zoomLevel.ToString(),
                            tileX + "_" + tileY + ".png");

                        using (var img = CutTile(rImg, x, y, w))
                        {
                            img.SaveToFile(tilePath);
                        }
                    }
                }
            }
        }

        private Bitmap CutTile(Bitmap rImg, int rX, int rZ, int scale)
        {
            var width = rImg.Width / scale;
            var height = rImg.Height / scale;

            var srcRect = new Rectangle(
                width * rX,
                height * rZ,
                width,
                height
            );

            var img = new Bitmap(Layer.Map.Meta.TileSize.Width, Layer.Map.Meta.TileSize.Height);
            using (var g = img.GetGraphics())
            {
                g.DrawImage(rImg, new Rectangle(0, 0, img.Width, img.Height), srcRect, GraphicsUnit.Pixel);

            }

            return img;
        }

        public Bitmap GetBitmap()
        {
            //return Bitmap;
            lock (_bitmapSync)
            {
                return (Bitmap)Bitmap.Clone();
            }
        }

        public void Update(BlockColumnMeta update)
        {
            var rect = GetBlockRectangle(update.Position);

            if (!BitmapBounds.Contains(rect))
            {
                Log.WarnFormat("Attemptedto draw block outside the tile bounds, Block: {2}, Rect: {0}, Bitmap Bounds : {1}", rect, BitmapBounds, update.Position);
                return;
            }

            Blocks[update.Position] = update;
            LastUpdated = DateTime.UtcNow;

            //using (var clip = new Region(rect))
            //{
            //Graphics.Clip = clip;
            Layer.Renderer.DrawBlock(Graphics, rect, update);

            foreach (var postProcessor in Layer.PostProcessors)
            {
                postProcessor.PostProcess(this, Graphics, update);
            }

            //Graphics.ResetClip();
            //}
        }

        public Rectangle GetBlockRectangle(BlockPosition pos)
        {
            var w = Layer.Renderer.RenderScale.Width;
            var h = Layer.Renderer.RenderScale.Height;

            var x = (pos.X % (1 << 9)) * w;
            var z = (pos.Z % (1 << 9)) * h;

            if (x < 0)
            {
                x = Bitmap.Width + x;
            }
            if (z < 0)
            {
                z = Bitmap.Height + z;
            }

            return new Rectangle(x, z, w, h);
        }

        public void Dispose()
        {
            //Log.InfoFormat("Saving Tile {0},{1}@{2}", Position.X, Position.Z, Position.Zoom);

            Save();
            Graphics?.Dispose();

            Bitmap?.Dispose();
            IsNew = false;
        }
    }
}
