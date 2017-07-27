using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MiMap.Common.Data;
using MiNET.Worlds;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing.Renderers.Base
{
    public class DefaultLayerRenderer : ILayerRenderer
    {

        public Color Background { get; } = Color.FromArgb(0x7F121212);
        public Size RenderScale { get; } = new Size(1, 1);

        private BiomeUtils BiomeUtils { get; }

        private readonly Dictionary<int, SolidBrush> _brushCache = new Dictionary<int, SolidBrush>();

        public DefaultLayerRenderer()
        {
            BiomeUtils = new BiomeUtils();
            BiomeUtils.PrecomputeBiomeColors();
        }

        private SolidBrush GetBrush(int biomeId)
        {
            SolidBrush brush;
            if (!_brushCache.TryGetValue(biomeId, out brush))
            {
                var biome = BiomeUtils.GetBiome(biomeId);
                var c = Color.FromArgb(biome.Foliage);
                brush = new SolidBrush(Color.FromArgb(255, c.R, c.G, c.B));
                _brushCache.Add(biomeId, brush);
            }

            return brush;
        }

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            graphics.FillRectangle(GetBrush(blockColumn.BiomeId), bounds);
        }

        public void Dispose()
        {
            foreach (var kvp in _brushCache.ToArray())
            {
                kvp.Value.Dispose();
                _brushCache.Remove(kvp.Key);
            }

        }
    }
}
