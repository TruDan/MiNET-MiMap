using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Utils;
using MiNET.Worlds;
using Size = BiomeMap.Drawing.Data.Size;

namespace BiomeMap.Drawing.Renderers.Base
{
    public class TextureLayerRenderer : ILayerRenderer
    {

        public Color Background { get; } = Color.FromArgb(0x7F121212);
        public Size RenderScale { get; } = new Size(16, 16);

        public static readonly byte[] FoilageBlocks = { 2, 18, 111, 161 };

        private TextureMap TextureMap { get; }
        private BiomeUtils BiomeUtils { get; }

        public TextureLayerRenderer()
        {
            TextureMap = new TextureMap();
            BiomeUtils = new BiomeUtils();
            BiomeUtils.PrecomputeBiomeColors();
        }

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            var blockId = (byte)blockColumn.BlockId;

            Bitmap texture = TextureMap.GetTexture(blockId);

            if (FoilageBlocks.Contains(blockId))
            {
                var biome = BiomeUtils.GetBiome(blockColumn.BiomeId);
                var c = Color.FromArgb(biome.Foliage);
                var tint = Color.FromArgb(255, c.R, c.G, c.B);
                using (var img = texture.Tint(tint))
                {
                    graphics.DrawImage(img, bounds);
                }
            }
            else
            {
                graphics.DrawImage(texture, bounds);
            }

        }

        public void Dispose()
        {
            TextureMap?.Dispose();
        }
    }
}
