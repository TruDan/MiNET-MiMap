using System.Drawing;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;

namespace BiomeMap.Drawing.Renderers
{
    public interface ILayerRenderer
    {
        void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn);
    }
}
