using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;
using log4net;

namespace BiomeMap.Drawing
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
            }
            
            Bitmap = bitmap;
            BitmapBounds = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            Graphics = Graphics.FromImage(bitmap);
        }

        private RectangleF GetBlockRectangle(BlockPosition blockPos)
        {

            var bounds = Position.GetBlockBounds();

            var xSize = bounds.Width / (float)Bitmap.Width;
            var ySize = bounds.Height / (float)Bitmap.Height;
            var x = (int)Math.Floor((Math.Abs(blockPos.X - bounds.Min.X) / (float)bounds.Width) * (Bitmap.Width - xSize));
            var y = (int)Math.Floor((Math.Abs(blockPos.Z - bounds.Min.Z) / (float)bounds.Height) * (Bitmap.Height - ySize));
            return new RectangleF(
                x, y,
                xSize, ySize
                );
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
        }
    }
}
