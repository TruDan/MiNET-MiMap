using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using log4net;
using MiMap.Drawing.Utils;
using MiNET.Blocks;

namespace MiMap.Drawing.Renderers.Texture
{
    public class ResourcePack : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourcePack));

        private static readonly string BlockBasePath = Path.Combine("assets", "minecraft", "textures", "blocks");

        private Bitmap _noTexture { get; }

        private int _resolution { get; } = 16;

        private Dictionary<byte, Bitmap> _textures = new Dictionary<byte, Bitmap>();

        private ZipArchive ResourcePackArchive { get; }

        public ResourcePack() : this(typeof(ResourcePack).Assembly.GetManifestResourceStream(typeof(ResourcePack).Namespace + ".default.zip"))
        {
        }

        public ResourcePack(string zipPath) : this(File.OpenRead(zipPath))
        {
        }
        public ResourcePack(Stream zipStream) : this(new ZipArchive(zipStream))
        {
        }

        public ResourcePack(ZipArchive archive)
        {
            _noTexture = CreateNoTexture();
            ResourcePackArchive = archive;
            LoadResourcePack();
        }

        private Bitmap CreateNoTexture()
        {
            Bitmap noTexture = new Bitmap(_resolution, _resolution);

            using (var bitmap = new Bitmap(2, 2))
            {

                bitmap.SetPixel(0, 0, Color.Magenta);
                bitmap.SetPixel(1, 1, Color.Magenta);
                bitmap.SetPixel(0, 1, Color.Black);
                bitmap.SetPixel(1, 0, Color.Black);

                using (var g = noTexture.GetGraphics())
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, noTexture.Width, noTexture.Height));
                }

            }

            return noTexture;
        }

        private void LoadResourcePack()
        {
            foreach (var kvp in BlockFactory.NameToId)
            {
                var name = kvp.Key;
                var id = kvp.Value;

                Bitmap img;
                if (!TryLoadBlockTexture(name + "_top", out img))
                {
                    if (!TryLoadBlockTexture(name, out img))
                    {
                        // fixes
                        if (id == 8)
                        {
                            TryLoadBlockTexture("water_flow", out img);
                        }
                        else if (id == 9)
                        {
                            TryLoadBlockTexture("water_still", out img);
                        }
                        else if (id == 10)
                        {
                            TryLoadBlockTexture("lava_still", out img);
                        }
                        else if (id == 11)
                        {
                            TryLoadBlockTexture("lava_still", out img);
                        }
                        else
                        {
                            img = _noTexture;
                        }
                    }
                    else
                    {
                        //Log.InfoFormat("TexturePack Loaded Block ID {0}\t({1}) - {2}", id, name, name);
                    }
                }
                else
                {
                    //Log.InfoFormat("TexturePack Loaded Block ID {0}\t({1}) - {2}", id, name, name + "_top");
                }

                _textures[id] = img;
            }
        }

        private bool TryLoadBlockTexture(string name, out Bitmap bitmap)
        {
            var path = Path.Combine(BlockBasePath, name);

            var entry = ResourcePackArchive.Entries.FirstOrDefault(e => e.Name.EndsWith(".png") && e.FullName.Replace('/', '\\').StartsWith(path, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
            {
                using (var zipStream = entry.Open())
                {
                    using (var img = Image.FromStream(zipStream))
                    {
                        var s = Math.Min(img.Width, img.Height);

                        bitmap = new Bitmap(_resolution, _resolution);

                        using (var g = bitmap.GetGraphics())
                        {
                            if (img.Width != img.Height)
                            {
                                var src = new Rectangle(0, 0, _resolution, _resolution);

                                if (img.Width >= (_resolution * 2))
                                {
                                    src.X = _resolution / 2;
                                }
                                if (img.Height >= (_resolution * 2))
                                {
                                    src.Y = _resolution / 2;
                                }

                                g.DrawImage(img, new Rectangle(0, 0, bitmap.Width, bitmap.Height), src,
                                    GraphicsUnit.Pixel);
                            }
                            else
                            {
                                g.DrawImage(img, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                            }
                        }

                        return true;
                    }
                }
            }

            bitmap = _noTexture;
            return false;
        }

        public TextureBrush GetTexture(byte blockId)
        {
            Bitmap bitmap;
            if (_textures.TryGetValue(blockId, out bitmap))
            {
                return new TextureBrush(bitmap);
            }

            return new TextureBrush(_noTexture);
        }

        public void Dispose()
        {
            _noTexture?.Dispose();
            ResourcePackArchive?.Dispose();
            foreach (var texture in _textures.ToArray())
            {
                texture.Value.Dispose();
                _textures.Remove(texture.Key);
            }
        }
    }
}