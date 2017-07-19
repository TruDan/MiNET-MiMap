using System;
using System.Drawing;
using BiomeMap.Drawing.Layers;
using BiomeMap.Shared.Data;

namespace BiomeMap.Drawing.Renderers
{
    public interface ILayerRenderer : IDisposable
    {
        void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn);
    }
}
