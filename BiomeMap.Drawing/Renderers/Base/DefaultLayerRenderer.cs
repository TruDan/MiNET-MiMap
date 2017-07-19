using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Layers;
using BiomeMap.Shared.Data;
using MiNET.Worlds;

namespace BiomeMap.Drawing.Renderers.Base
{
    public class DefaultLayerRenderer : ILayerRenderer
    {
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
