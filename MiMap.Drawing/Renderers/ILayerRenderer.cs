using System;
using System.Drawing;
using MiMap.Common.Data;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing.Renderers
{
    public interface ILayerRenderer : IDisposable
    {

        Color Background { get; }

        Size RenderScale { get; }

        void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn);

    }
}
