using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Shared.Data;

namespace BiomeMap.Drawing.Renderers.Overlay
{
    public class HeightMapOverlayRenderer : ILayerRenderer
    {
        public const int Steps = 25;
        public const float MaxOpacity = 0.5f;

        private SolidBrush[] Brushes { get; }

        public HeightMapOverlayRenderer()
        {
            var alphaStep = 255f / Steps;

            Brushes = new SolidBrush[Steps];

            for (int i = 0; i < Steps; i++)
            {
                var c = Math.Min(255, (int)Math.Round(alphaStep * i));
                Debug.WriteLine("Brush {0}/{1}: argb({2},{3},{4},{5})", i, Steps, (int)(c * MaxOpacity), c, c, c);
                Brushes[i] = new SolidBrush(Color.FromArgb((int)(c * MaxOpacity), c, c, c));
            }
        }



        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            var i = blockColumn.Height % Steps;
            graphics.FillRectangle(Brushes[i], bounds);
        }


        public void Dispose()
        {
            foreach (var b in Brushes)
            {
                b.Dispose();
            }
        }
    }
}
