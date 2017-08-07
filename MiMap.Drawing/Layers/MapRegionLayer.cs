using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using log4net;
using MiMap.Common;
using MiMap.Common.Data;
using MiMap.Drawing.Layers;
using MiMap.Drawing.Utils;

namespace MiMap.Drawing
{
    public class MapRegionLayer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MapRegionLayer));


        public MapLayer Layer { get; }

        public RegionPosition Position { get; }

        public DateTime LastUpdated { get; private set; }

        private string FilePath { get; }

        private Bitmap Bitmap { get; set; }
        private readonly object _bitmapSync = new object();
        private readonly object _ioSync = new object();

        private RectangleF BitmapBounds { get; set; }

        private Graphics Graphics { get; set; }

        private bool IsNew { get; set; }

        public BlockPosition[] UpdatedBlocks => _updatedBlocks.ToArray();
        private readonly List<BlockPosition> _updatedBlocks = new List<BlockPosition>();

        public MapRegionLayer(MapLayer layer, RegionPosition pos)
        {
            Layer = layer;
            Position = pos;
            FilePath = Path.Combine(layer.Directory, ".regions",
                $"{Position.X}_{Position.Z}.png");
            Load();
        }

        private void Load()
        {
            lock (_ioSync)
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
                    bitmap = new Bitmap(Layer.Renderer.RenderScale.Width * (1 << 9),
                        Layer.Renderer.RenderScale.Height * (1 << 9));
                    IsNew = true;
                }

                lock (_bitmapSync)
                {
                    Bitmap = bitmap;
                    BitmapBounds = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
                    Graphics = bitmap.GetGraphics();

                    if (IsNew)
                    {
                        Graphics.Clear(Layer.Renderer.Background);
                    }
                }
            }
        }

        public void Save()
        {
            lock (_ioSync)
            {
                lock (_bitmapSync)
                {
                    using (var img = GetBitmap())
                    {
                        img.SaveToFile(FilePath);
                    }
                }
            }
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

            //using (var clip = new Region(rect))
            //{
            //Graphics.Clip = clip;
            lock (_bitmapSync)
            {
                Layer.Renderer.DrawBlock(Graphics, rect, update);

                foreach (var postProcessor in Layer.PostProcessors)
                {
                    postProcessor.PostProcess(this, Graphics, update);
                }
            }

            _updatedBlocks.Add(update.Position);

            //Graphics.ResetClip();
            //}
        }

        public void ClearUpdatedBlocks()
        {
            _updatedBlocks.Clear();
        }

        public Rectangle GetBlockRectangle(BlockPosition pos)
        {
            var w = Layer.Renderer.RenderScale.Width;
            var h = Layer.Renderer.RenderScale.Height;

            var x = (pos.X % (1 << 9)) * w;
            var z = (pos.Z % (1 << 9)) * h;

            if (x < 0)
            {
                x = (int)BitmapBounds.Width + x;
            }
            if (z < 0)
            {
                z = (int)BitmapBounds.Height + z;
            }

            return new Rectangle(x, z, w, h);
        }

        public void Dispose()
        {
            //Log.InfoFormat("Saving Tile {0},{1}@{2}", Position.X, Position.Z, Position.Zoom);

            //Save();
            Graphics?.Dispose();

            Bitmap?.Dispose();
            IsNew = false;
        }
    }
}
