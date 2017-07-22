using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using Size = BiomeMap.Drawing.Data.Size;

namespace BiomeMap.Drawing.Renderers.Overlay
{
    public class BiomeOverlayRenderer : ILayerRenderer
    {

        public const int Alpha = 128;

        public Color Background { get; } = Color.Transparent;
        public Size RenderScale { get; } = new Size(1,1);

        private static readonly Dictionary<byte, Color> _biomeColors = new Dictionary<byte, Color>();


        static BiomeOverlayRenderer()
        {
            _biomeColors = new Dictionary<byte, Color>()
            {
                {0, Color.FromArgb(0x7F000070)},
                {1, Color.FromArgb(0x7F8DB360)},
                {2, Color.FromArgb(0x7FFA9418)},
                {3, Color.FromArgb(0x7F606060)},
                {4, Color.FromArgb(0x7F056621)},
                {5, Color.FromArgb(0x7F0B6659)},
                {6, Color.FromArgb(0x7F07F9B2)},
                {7, Color.FromArgb(0x7F0000FF)},
                {8, Color.FromArgb(0x7FFF0000)},
                {9, Color.FromArgb(0x7F8080FF)},
                {10, Color.FromArgb(0x7F9090A0)},
                {11, Color.FromArgb(0x7FA0A0FF)},
                {12, Color.FromArgb(0x7FFFFFFF)},
                {13, Color.FromArgb(0x7FA0A0A0)},
                {14, Color.FromArgb(0x7FFF00FF)},
                {15, Color.FromArgb(0x7FA000FF)},
                {16, Color.FromArgb(0x7FFADE55)},
                {17, Color.FromArgb(0x7FD25F12)},
                {18, Color.FromArgb(0x7F22551C)},
                {19, Color.FromArgb(0x7F163933)},
                {20, Color.FromArgb(0x7F72789A)},
                {21, Color.FromArgb(0x7F537B09)},
                {22, Color.FromArgb(0x7F2C4205)},
                {23, Color.FromArgb(0x7F628B17)},
                {24, Color.FromArgb(0x7F000030)},
                {25, Color.FromArgb(0x7FA2A284)},
                {26, Color.FromArgb(0x7FFAF0C0)},
                {27, Color.FromArgb(0x7F307444)},
                {28, Color.FromArgb(0x7F1F5F32)},
                {29, Color.FromArgb(0x7F40511A)},
                {30, Color.FromArgb(0x7F31554A)},
                {31, Color.FromArgb(0x7F243F36)},
                {32, Color.FromArgb(0x7F596651)},
                {33, Color.FromArgb(0x7F545F3E)},
                {34, Color.FromArgb(0x7F507050)},
                {35, Color.FromArgb(0x7FBDB25F)},
                {36, Color.FromArgb(0x7FA79D64)},
                {37, Color.FromArgb(0x7FD94515)},
                {38, Color.FromArgb(0x7FB09765)},
                {39, Color.FromArgb(0x7FCA8C65)},
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
