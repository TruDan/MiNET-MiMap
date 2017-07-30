using System.Collections.Generic;
using System.Drawing;
using MiMap.Common.Data;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing.Renderers.Overlay
{
    public class BiomeOverlayRenderer : ILayerRenderer
    {

        public const int Alpha = 128;

        public Color Background { get; } = Color.Transparent;
        public Size RenderScale { get; } = new Size(1, 1);

        private static readonly Dictionary<byte, Color> _biomeColors = new Dictionary<byte, Color>();



        static BiomeOverlayRenderer()
        {
            _biomeColors = new Dictionary<byte, Color>()
            {{0, Color.FromArgb(0xFF, 0x00, 0x00, 0x70)},
                {1, Color.FromArgb(0xFF, 0x8D, 0xB3, 0x60)},
                {2, Color.FromArgb(0xFF, 0xFA, 0x94, 0x18)},
                {3, Color.FromArgb(0xFF, 0x60, 0x60, 0x60)},
                {4, Color.FromArgb(0xFF, 0x05, 0x66, 0x21)},
                {5, Color.FromArgb(0xFF, 0x0B, 0x66, 0x59)},
                {6, Color.FromArgb(0xFF, 0x07, 0xF9, 0xB2)},
                {7, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF)},
                {8, Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)},
                {9, Color.FromArgb(0xFF, 0x80, 0x80, 0xFF)},
                {10, Color.FromArgb(0xFF, 0x90, 0x90, 0xA0)},
                {11, Color.FromArgb(0xFF, 0xA0, 0xA0, 0xFF)},
                {12, Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)},
                {13, Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)},
                {14, Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF)},
                {15, Color.FromArgb(0xFF, 0xA0, 0x00, 0xFF)},
                {16, Color.FromArgb(0xFF, 0xFA, 0xDE, 0x55)},
                {17, Color.FromArgb(0xFF, 0xD2, 0x5F, 0x12)},
                {18, Color.FromArgb(0xFF, 0x22, 0x55, 0x1C)},
                {19, Color.FromArgb(0xFF, 0x16, 0x39, 0x33)},
                {20, Color.FromArgb(0xFF, 0x72, 0x78, 0x9A)},
                {21, Color.FromArgb(0xFF, 0x53, 0x7B, 0x09)},
                {22, Color.FromArgb(0xFF, 0x2C, 0x42, 0x05)},
                {23, Color.FromArgb(0xFF, 0x62, 0x8B, 0x17)},
                {24, Color.FromArgb(0xFF, 0x00, 0x00, 0x30)},
                {25, Color.FromArgb(0xFF, 0xA2, 0xA2, 0x84)},
                {26, Color.FromArgb(0xFF, 0xFA, 0xF0, 0xC0)},
                {27, Color.FromArgb(0xFF, 0x30, 0x74, 0x44)},
                {28, Color.FromArgb(0xFF, 0x1F, 0x5F, 0x32)},
                {29, Color.FromArgb(0xFF, 0x40, 0x51, 0x1A)},
                {30, Color.FromArgb(0xFF, 0x31, 0x55, 0x4A)},
                {31, Color.FromArgb(0xFF, 0x24, 0x3F, 0x36)},
                {32, Color.FromArgb(0xFF, 0x59, 0x66, 0x51)},
                {33, Color.FromArgb(0xFF, 0x54, 0x5F, 0x3E)},
                {34, Color.FromArgb(0xFF, 0x50, 0x70, 0x50)},
                {35, Color.FromArgb(0xFF, 0xBD, 0xB2, 0x5F)},
                {36, Color.FromArgb(0xFF, 0xA7, 0x9D, 0x64)},
                {37, Color.FromArgb(0xFF, 0xD9, 0x45, 0x15)},
                {38, Color.FromArgb(0xFF, 0xB0, 0x97, 0x65)},
                {39, Color.FromArgb(0xFF, 0xCA, 0x8C, 0x65)},
                {127, Color.Transparent}
            };
        }

        public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
        {
            var color = GetBiomeColor(blockColumn.BiomeId);

            using (var b = new SolidBrush(color))
            {
                graphics.FillRectangle(b, bounds);
            }
        }

        private static Color GetBiomeColor(byte biomeId)
        {
            Color color;
            if (_biomeColors.TryGetValue(biomeId, out color))
            {
                return color;
            }
            return Color.Transparent;
        }

        public void Dispose()
        {

        }
    }
}
