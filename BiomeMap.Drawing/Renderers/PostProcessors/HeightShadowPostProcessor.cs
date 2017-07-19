using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Utils;

namespace BiomeMap.Drawing.Renderers.PostProcessors
{
    public class HeightShadowPostProcessor : IPostProcessor
    {

        public void PostProcess(MapRegionLayer layer, Graphics graphics)
        {
            //ShadowSize = (int)Math.Max(1, (layer.Layer.Renderer.RenderScale.Width * 0.25f));

            var clipRegion = new Region();
            clipRegion.MakeInfinite();

            var ordered = layer.Blocks.Values.GroupBy(b => b.Height).OrderByDescending(g => g.Key).ToArray();

            var queue = new List<BlockColumnMeta>(layer.Blocks.Values);

            foreach (var group in ordered)
            {

                //graphics.Clip = clipRegion;

                foreach (var orderedBlock in group.ToArray())
                {
                    var top = queue.FirstOrDefault(b => b.Position.X == orderedBlock.Position.X &&
                                                        b.Position.Z == orderedBlock.Position.Z - 1);
                    var left = queue.FirstOrDefault(b => b.Position.X == orderedBlock.Position.X - 1 &&
                                                        b.Position.Z == orderedBlock.Position.Z);
                    var right = queue.FirstOrDefault(b => b.Position.X == orderedBlock.Position.X + 1 &&
                                                        b.Position.Z == orderedBlock.Position.Z);
                    var bottom = queue.FirstOrDefault(b => b.Position.X == orderedBlock.Position.X &&
                                                        b.Position.Z == orderedBlock.Position.Z + 1);

                    DrawShadow(layer, graphics, orderedBlock,
                        top != null ? Math.Max(0, orderedBlock.Height - top.Height) : 0,
                        left != null ? Math.Max(0, orderedBlock.Height - left.Height) : 0,
                        right != null ? Math.Max(0, orderedBlock.Height - right.Height) : 0,
                        bottom != null ? Math.Max(0, orderedBlock.Height - bottom.Height) : 0);
                }

                foreach (var orderedBlock in group.ToArray())
                {
                    //clipRegion.Exclude(layer.GetBlockRectangle(orderedBlock.Position));
                    //queue.Remove(orderedBlock);
                }

                //graphics.ResetClip();
            }
        }

        private const float AlphaMultiplier = 255f / 8f;

        private const float ShadowSizeMultiplier = 8f / MaxShadowSize;
        private const float MaxShadowSize = 3f;

        private void DrawShadow(MapRegionLayer layer, Graphics graphics, BlockColumnMeta column, int sizeTop,
            int sizeLeft, int sizeRight, int sizeBottom)
        {
            var rect = layer.GetBlockRectangle(column.Position);


            if (sizeTop > 0)
            {
                var size = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeTop * ShadowSizeMultiplier, 0f, 1f))), 1f, MaxShadowSize);
                for (int i = 1; i <= size; i++)
                {
                    using (var pen = new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeTop * AlphaMultiplier, 8f, 255f) / size) * i
                        , Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left, rect.Top - i, rect.Right, rect.Top - i);
                    }
                }
            }

            if (sizeLeft > 0)
            {
                var size = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeLeft * ShadowSizeMultiplier, 0f, 1f))), 1f, MaxShadowSize);
                for (int i = 1; i <= size; i++)
                {
                    using (var pen = new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeLeft * AlphaMultiplier, 8f, 255f) / size) * i,
                        Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left - i, rect.Top, rect.Left - i, rect.Bottom);
                    }
                }
            }

            if (sizeRight > 0)
            {
                var size = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeRight * ShadowSizeMultiplier, 0f, 1f))), 1f, MaxShadowSize);
                for (int i = 1; i <= size; i++)
                {
                    using (var pen =
                        new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeRight * AlphaMultiplier, 8f, 255f) / size) * i,
                            Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Right + i, rect.Top, rect.Right + i, rect.Bottom);
                    }
                }
            }

            if (sizeBottom > 0)
            {
                var size = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeBottom * ShadowSizeMultiplier, 0f, 1f))), 1f, MaxShadowSize);
                for (int i = 1; i <= size; i++)
                {
                    using (var pen =
                        new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeBottom * AlphaMultiplier, 8f, 255f) / size) * i,
                            Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left, rect.Bottom + i, rect.Right, rect.Bottom + i);
                    }
                }
            }
        }
    }
}
