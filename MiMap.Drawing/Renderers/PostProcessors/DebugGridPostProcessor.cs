using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Common.Data;
using MiMap.Drawing.Utils;

namespace MiMap.Drawing.Renderers.PostProcessors
{
    public class DebugGridPostProcessor : IPostProcessor
    {
        private readonly Pen _gridPen;
        private readonly Pen _chunkPen;

        private readonly Font _chunkCoordFont;
        private readonly Font _coordFont;
        private readonly Brush _coordForeground;
        private readonly Brush _coordShadow;



        public DebugGridPostProcessor()
        {
            _gridPen = new Pen(Color.FromArgb((int)(0.15 * 255f), Color.Black));
            _chunkPen = new Pen(Color.FromArgb((int)(0.25 * 255f), 0xff, 0x00, 0xff), 3f);
            _chunkCoordFont = new Font(FontFamily.GenericMonospace, 14f, FontStyle.Bold, GraphicsUnit.Pixel);
            _coordFont = new Font(FontFamily.GenericMonospace, 10f, FontStyle.Bold, GraphicsUnit.Pixel);
            _coordForeground = new SolidBrush(Color.White);
            _coordShadow = new SolidBrush(Color.Black);
        }


        public void PostProcess(MapRegionLayer layer, Graphics graphics, BlockColumnMeta block)
        {
            var rect = layer.GetBlockRectangle(block.Position);

            var chunkPos = block.Position.GetChunkPosition();
            var chunkBounds = chunkPos.GetBlockBounds();

            var chunkRect = layer.GetChunkRectangle(chunkPos);

            var rX = block.Position.X % 16;
            var rZ = block.Position.Z % 16;


            if (rX == 15 && rZ == 15)
            {
                graphics.DrawRectangle(_chunkPen, chunkRect);
            }

            graphics.DrawRectangle(_gridPen, rect);

            if (rX == 15 && rZ == 15)
            {
                DrawText(graphics, chunkPos.ToString(), chunkRect, _chunkCoordFont);

                var min = chunkBounds.Min;
                var max = chunkBounds.Max;

                var tl = min;
                var tr = new BlockPosition(min.X, max.Z);
                var bl = new BlockPosition(max.X, min.Z);
                var br = max;

                DrawText(graphics, tl.ToString(), chunkRect, _coordFont, StringAlignment.Near, StringAlignment.Near);
                DrawText(graphics, tr.ToString(), chunkRect, _coordFont, StringAlignment.Far, StringAlignment.Near);
                DrawText(graphics, bl.ToString(), chunkRect, _coordFont, StringAlignment.Near, StringAlignment.Far);
                DrawText(graphics, br.ToString(), chunkRect, _coordFont, StringAlignment.Far, StringAlignment.Far);
            }

        }

        private void DrawText(Graphics graphics, string text, Rectangle rect, Font font, StringAlignment horizontAlignment = StringAlignment.Center, StringAlignment verticalAlignment = StringAlignment.Center)
        {
            var chunkRectShadow = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width,
                rect.Height);
            graphics.DrawString(text, font, _coordShadow, chunkRectShadow, new StringFormat(StringFormatFlags.NoWrap)
            {
                Alignment = horizontAlignment,
                LineAlignment = verticalAlignment,
                Trimming = StringTrimming.EllipsisCharacter
            });

            graphics.DrawString(text, font, _coordForeground, rect, new StringFormat(StringFormatFlags.NoWrap)
            {
                Alignment = horizontAlignment,
                LineAlignment = verticalAlignment,
                Trimming = StringTrimming.EllipsisCharacter
            });

        }
    }
}
