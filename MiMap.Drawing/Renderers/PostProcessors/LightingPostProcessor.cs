using System.Drawing;
using MiMap.Common.Data;
using MiMap.Drawing.Utils;

namespace MiMap.Drawing.Renderers.PostProcessors
{
    public class LightingPostProcessor : IPostProcessor
    {
        private const float MaxLightLevel = 15f;
        private const float MaxOverlayAlpha = 255f;

        public void PostProcess(MapRegionLayer layer, Graphics graphics, BlockColumnMeta block)
        {
            var rect = layer.GetBlockRectangle(block.Position);

            var alpha = (int)MathUtils.Clamp(MaxOverlayAlpha * ((MaxLightLevel - block.LightLevel) / MaxLightLevel), 0f, MaxOverlayAlpha);

            using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
            {
                graphics.FillRectangle(brush, rect);
            }
        }
    }
}
