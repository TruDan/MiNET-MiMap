using System;
using System.Drawing;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;
using Size = BiomeMap.Drawing.Data.Size;

namespace BiomeMap.Drawing.Renderers
{
    public interface ILayerRenderer : IDisposable
    {

        Color Background { get; }

        Size RenderScale { get; }

        void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn);

    }
}
