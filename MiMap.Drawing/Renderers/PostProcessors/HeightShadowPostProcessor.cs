using System;
using System.Drawing;
using MiMap.Common.Data;
using MiMap.Drawing.Utils;

namespace MiMap.Drawing.Renderers.PostProcessors
{
    public class HeightShadowPostProcessor : IPostProcessor
    {

        public void PostProcess(MapRegionLayer layer, Graphics graphics, BlockColumnMeta block)
        {
            DrawShadow(layer, graphics, block,
                Math.Max(GetHeightDiff(layer, block, 0, -1), 0),
                Math.Max(GetHeightDiff(layer, block, -1, 0), 0),
                Math.Max(GetHeightDiff(layer, block, 1, 0), 0),
                Math.Max(GetHeightDiff(layer, block, 0, 1), 0));
        }

        private int GetHeightDiff(MapRegionLayer regionLayer, BlockColumnMeta block, int xOffset, int zOffset)
        {
            var pos = new BlockPosition(block.Position.X + xOffset, block.Position.Z + zOffset);

            BlockColumnMeta targetBlock = regionLayer.Layer.Map.GetRegionLayer(pos.GetRegionPosition()).GetBlockData(pos);
            if (targetBlock != null)
            {
                return targetBlock.Height - block.Height;
            }

            return 0;
        }

        /*
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
        */
        private const float AlphaMultiplier = 255f / 8f;

        private const float ShadowSizeMultiplier = 8f / MaxShadowSize;
        private const float MaxShadowSize = 3f;

        private void DrawShadow(MapRegionLayer layer, Graphics graphics, BlockColumnMeta column, int sizeTop,
            int sizeLeft, int sizeRight, int sizeBottom)
        {
            var rect = layer.GetBlockRectangle(column.Position);

            int actSizeTop = 0, actSizeLeft = 0, actSizeRight = 0, actSizeBottom = 0;

            if (sizeTop > 0)
            {
                actSizeTop = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeTop * ShadowSizeMultiplier, 0.25f, 1f))), 1f, MaxShadowSize);
                for (int i = 0; i < actSizeTop; i++)
                {
                    using (var pen = new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeTop * AlphaMultiplier, 64f, 255f) / actSizeTop) * (i + 1)
                        , Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left, rect.Top + i, rect.Right, rect.Top + i);
                    }
                }
            }

            if (sizeBottom > 0)
            {
                actSizeBottom = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeBottom * ShadowSizeMultiplier, 0.25f, 1f))), 1f, MaxShadowSize);
                for (int i = 0; i < actSizeBottom; i++)
                {
                    using (var pen =
                        new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeBottom * AlphaMultiplier, 64f, 255f) / actSizeBottom) * (i + 1),
                            Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left, rect.Bottom - actSizeBottom + i, rect.Right, rect.Bottom - actSizeBottom + i);
                    }
                }
            }

            if (sizeLeft > 0)
            {
                actSizeLeft = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeLeft * ShadowSizeMultiplier, 0.25f, 1f))), 1f, MaxShadowSize);
                for (int i = 0; i < actSizeLeft; i++)
                {
                    using (var pen = new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeLeft * AlphaMultiplier, 64f, 255f) / actSizeLeft) * (i + 1),
                        Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Left + i, rect.Top, rect.Left + i, rect.Bottom);
                    }
                }
            }

            if (sizeRight > 0)
            {
                actSizeRight = (int)MathUtils.Clamp(Math.Round(MaxShadowSize * (MathUtils.Clamp(sizeRight * ShadowSizeMultiplier, 0.25f, 1f))), 1f, MaxShadowSize);
                for (int i = 0; i < actSizeRight; i++)
                {
                    using (var pen =
                        new Pen(Color.FromArgb((int)(MathUtils.Clamp(sizeRight * AlphaMultiplier, 64f, 255f) / actSizeRight) * (i + 1),
                            Color.Black)))
                    {
                        graphics.DrawLine(pen, rect.Right - actSizeRight + i, rect.Top, rect.Right - actSizeRight + i, rect.Bottom);
                    }
                }
            }
        }
    }
}
