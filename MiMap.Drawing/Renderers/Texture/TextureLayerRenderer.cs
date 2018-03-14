using System.Drawing;
using System.Linq;
using MiMap.Common.Data;
using MiMap.Drawing.Renderers.Texture;
using MiNET.Worlds;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing.Renderers.Base
{
    public class TextureLayerRenderer : ILayerRenderer
    {

        public Color Background { get; } = Color.Transparent;
        public Size RenderScale { get; } = new Size(16, 16);

        public static readonly byte[] FoilageBlocks = { 2, 18, 111, 161 };

        private Texture.ResourcePack ResourcePack { get; }
        private BiomeUtils BiomeUtils { get; }

        public TextureLayerRenderer(TextureRendererConfig config)
        {
            if(config.ResourcePack.Equals("default", System.StringComparison.InvariantCultureIgnoreCase))
            {
                ResourcePack = new Texture.ResourcePack();
            }
            else
            {
                ResourcePack = new Texture.ResourcePack(config.ResourcePack);
            }

            BiomeUtils = new BiomeUtils(); 
            BiomeUtils.PrecomputeBiomeColors();
        }

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            var blockId = (byte)blockColumn.BlockId;

            using (var texture = ResourcePack.GetTexture(blockId))
            {
                graphics.FillRectangle(texture, bounds);

                if (FoilageBlocks.Contains(blockId))
                {
                    var biome = BiomeUtils.GetBiome(blockColumn.BiomeId);
                    var c = Color.FromArgb(biome.Foliage);
                    var tint = Color.FromArgb(128, c.R, c.G, c.B);
                    using (var img = new SolidBrush(tint))
                    {
                        graphics.FillRectangle(img, bounds);
                    }
                }
            }

        }

        public void Dispose()
        {
            ResourcePack?.Dispose();
        }
    }
}
