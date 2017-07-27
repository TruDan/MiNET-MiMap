using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Common.Data;
using Size = BiomeMap.Common.Data.Size;

namespace BiomeMap.Drawing.Renderers.Overlay
{
    public class HeightMapOverlayRenderer : ILayerRenderer
    {
        public const int Steps = 25;
        public const float MaxOpacity = 0.5f;

        private Color[] Colors { get; }

        public HeightMapOverlayRenderer()
        {
            var alphaStep = 255f / Steps;

            Colors = new Color[Steps];

            for (int i = 0; i < Steps; i++)
            {
                var c = Math.Min(255, (int)Math.Round(alphaStep * i));

                Debug.WriteLine("Brush {0}/{1}: argb({2},{3},{4},{5})", i, Steps, (int)((Math.Abs(i - (Steps / 2)) * 255f) * MaxOpacity), c, c, c);

                Colors[i] = Color.FromArgb((int)(c * MaxOpacity), c, c, c);
            }
        }


        public Color Background { get; } = Color.Transparent;
        public Size RenderScale { get; } = new Size(1, 1);

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            var i = blockColumn.Height % Steps;
            using (var brush = new SolidBrush(Colors[i]))
            {
                graphics.FillRectangle(brush, bounds);
            }

        }


        public void Dispose()
        {

        }
    }
}
