using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using log4net;
using MiNET.Blocks;

namespace BiomeMap.Drawing.Renderers.Base
{
    public class TextureMap : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TextureMap));

        private static readonly string BlockBasePath = Path.Combine("assets", "minecraft", "textures", "blocks");

        private Bitmap _noTexture { get; }

        private Dictionary<byte, Bitmap> _textures = new Dictionary<byte, Bitmap>();

        private ZipArchive ResourcePack { get; }

        public TextureMap() : this(new MemoryStream(Textures.PureBDCraft_x16))
        {

        }

        public TextureMap(string zipPath) : this(File.OpenRead(zipPath))
        {
        }
        public TextureMap(Stream zipStream) : this(new ZipArchive(zipStream))
        {
        }

        public TextureMap(ZipArchive archive)
        {
            _noTexture = CreateNoTexture();
            ResourcePack = archive;
            LoadResourcePack();
        }

        private Bitmap CreateNoTexture()
        {
            var bitmap = new Bitmap(2, 2);

            bitmap.SetPixel(0, 0, Color.Magenta);
            bitmap.SetPixel(1, 1, Color.Magenta);
            bitmap.SetPixel(0, 1, Color.Black);
            bitmap.SetPixel(1, 0, Color.Black);

            return bitmap;
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
                        img = _noTexture;
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

            var entry = ResourcePack.Entries.FirstOrDefault(e => e.Name.EndsWith(".png") && e.FullName.Replace('/', '\\').StartsWith(path, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
            {
                using (var zipStream = entry.Open())
                {
                    using (var img = Image.FromStream(zipStream))
                    {
                        bitmap = new Bitmap(img);
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
            ResourcePack?.Dispose();
            foreach (var texture in _textures.ToArray())
            {
                texture.Value.Dispose();
                _textures.Remove(texture.Key);
            }
        }
    }
}