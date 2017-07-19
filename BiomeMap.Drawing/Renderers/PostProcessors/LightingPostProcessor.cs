using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Utils;

namespace BiomeMap.Drawing.Renderers.PostProcessors
{
    public class LightingPostProcessor : IPostProcessor
    {
        private const float MaxLightLevel = 15f;
        private const float MaxOverlayAlpha = 255f;

        public void PostProcess(MapRegionLayer layer, Graphics graphics)
        {
            foreach (var block in layer.Blocks.Values.ToArray())
            {
                DrawLighting(layer, graphics, block);
            }
        }

        private void DrawLighting(MapRegionLayer layer, Graphics graphics, BlockColumnMeta column)
        {
            var rect = layer.GetBlockRectangle(column.Position);

            var alpha = (int)MathUtils.Clamp(MaxOverlayAlpha * ((MaxLightLevel - column.LightLevel) / MaxLightLevel), 0f, MaxOverlayAlpha);

            using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
            {
                graphics.FillRectangle(brush, rect);
            }
        }
    }
}
