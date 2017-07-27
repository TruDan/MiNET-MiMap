using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using log4net;
using MiMap.Common.Data;
using MiMap.Drawing.Layers;

namespace MiMap.Drawing
{
    public class MapTile : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MapTile));

        public IMapLayer Layer { get; }

        public TilePosition Position { get; }

        private string FilePath { get; }

        private Bitmap Bitmap { get; set; }

        private RectangleF BitmapBounds { get; set; }

        private Graphics Graphics { get; set; }

        private bool IsNew { get; set; }

        public MapTile(IMapLayer layer, TilePosition pos)
        {
            Layer = layer;
            Position = pos;
            FilePath = Path.Combine(layer.Directory, Position.Zoom.ToString(),
                $"{Position.X}_{Position.Z}.png");
            Load();
        }

        private void Load()
        {
            //Log.InfoFormat("Loading Tile {0},{1}@{2}", Position.X, Position.Z, Position.Zoom);
            Bitmap bitmap;
            if (File.Exists(FilePath))
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var img = Image.FromStream(fs))
                    {
                        bitmap = new Bitmap(img);
                    }
                }
            }
            else
            {
                bitmap = new Bitmap(Layer.Map.Meta.TileSize.Width, Layer.Map.Meta.TileSize.Height);
                IsNew = true;
            }

            Bitmap = bitmap;
            BitmapBounds = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            Graphics = Graphics.FromImage(bitmap);
            Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Graphics.CompositingQuality = CompositingQuality.HighQuality;
            Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Graphics.PageUnit = GraphicsUnit.Pixel;

            if (IsNew)
            {
                Graphics.Clear(Layer.Renderer.Background);
            }
        }

        private RectangleF GetBlockRectangle(BlockPosition blockPos)
        {
            var bounds = Position.GetBlockBounds();

            var xSize = Bitmap.Width / (float)bounds.Width;
            var ySize = Bitmap.Height / (float)bounds.Height;
            var x = ((blockPos.X % bounds.Width) / (float)bounds.Width) * Bitmap.Width;
            var y = ((blockPos.Z % bounds.Height) / (float)bounds.Height) * Bitmap.Height;

            if (x < 0)
            {
                x = Bitmap.Width + x;
            }
            if (y < 0)
            {
                y = Bitmap.Height + y;
            }

            var rect = new RectangleF(
                x, y,
                xSize, ySize
            );

            if (!BitmapBounds.Contains(rect))
            {
                Log.WarnFormat("Attempted to draw block outside the tile bounds, Block: {2}, Rect: {0}, Bitmap Bounds : {1}", rect, BitmapBounds, blockPos);
            }

            return rect;
        }

        public void Update(BlockColumnMeta update)
        {
            var rect = GetBlockRectangle(update.Position);

            if (!BitmapBounds.Contains(rect))
            {
                Log.WarnFormat("Attemptedto draw block outside the tile bounds, Block: {2}, Rect: {0}, Bitmap Bounds : {1}", rect, BitmapBounds, update.Position);
                return;
            }

            Graphics.Clip = new Region(rect);
            Layer.Renderer.DrawBlock(Graphics, rect, update);
            Graphics.ResetClip();
        }

        public void Dispose()
        {
            //Log.InfoFormat("Saving Tile {0},{1}@{2}", Position.X, Position.Z, Position.Zoom);

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            using (var ms = new MemoryStream())
            {
                Bitmap.Save(ms, ImageFormat.Png);
                using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    byte[] bytes = ms.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            Bitmap?.Dispose();
            Graphics?.Dispose();
            IsNew = false;
        }
    }
}
