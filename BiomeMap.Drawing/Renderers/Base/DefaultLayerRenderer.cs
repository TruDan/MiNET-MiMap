using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Layers;

namespace BiomeMap.Drawing.Renderers.Base
{
    public class DefaultLayerRenderer : ILayerRenderer
    {
        public DefaultLayerRenderer()
        {
        }

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {

            graphics.FillRectangle(Brushes.DarkTurquoise, bounds);
        }
    }
}
