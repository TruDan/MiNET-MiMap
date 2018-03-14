using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;
using MiMap.Common.Data;
using MiMap.Drawing.Utils;
using MiMap.ResourcePackLib;
using MiMap.ResourcePackLib.Json;
using MiMap.ResourcePackLib.Json.BlockStates;
using MiMap.ResourcePackLib.Json.Models;
using MiNET.Blocks;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing.Renderers.ResourcePack
{
	class ResourcePackLayerRenderer : ILayerRenderer
	{
		public Color Background { get; } = Color.Transparent;
		public Size RenderScale { get; } = new Size(16, 16);

		public MinecraftResourcePack ResourcePack { get; }

		public ResourcePackLayerRenderer(ResourcePackRendererConfig config)
		{
			if (config.ResourcePack.Equals("default", System.StringComparison.InvariantCultureIgnoreCase))
			{
				ResourcePack = new MinecraftResourcePack();
			}
			else
			{
				ResourcePack = new MinecraftResourcePack(config.ResourcePack);
			}
		}

		private BlockStateVariant GetBlockStateVariant(BlockColumnMeta blockColumnMeta)
		{
		    if (ResourcePack.TryGetBlockState(blockColumnMeta.BlockId, blockColumnMeta.BlockMeta, out var blockStateVariant))
		    {
		        return blockStateVariant;
		    }

		    return null;
		}


		public void DrawBlock(Graphics graphics, RectangleF bounds, BlockColumnMeta blockColumn)
		{
		    var variant = GetBlockStateVariant(blockColumn);
		    if (variant == null) return;


			foreach (var model in variant.Select(m => m.Model))
			{
				foreach (var modelElement in model.Elements.OrderBy(e => e.To.Y))
				{
					var from = new PointF(modelElement.From.X, modelElement.From.Z);
					var to = new PointF(modelElement.To.X, modelElement.To.Z);
					var relativeBounds = new RectangleF(from, new System.Drawing.SizeF(to.X-from.X, to.Y-from.Y));

					var rect = new RectangleF(bounds.X + relativeBounds.X, bounds.Y + relativeBounds.Y, relativeBounds.Width, relativeBounds.Height);

					if (modelElement.Faces.TryGetValue(BlockFace.Up, out var face))
					{
						var srcRect = new Rectangle(face.UV.X1, face.UV.Y1, face.UV.X2-face.UV.X1, face.UV.Y2-face.UV.Y1);
						using (var br = new TextureBrush(face.Texture, srcRect))
						{
							graphics.FillRectangle(br, rect);
						}
					}
				}
			}
		}

		public void Dispose()
		{
			ResourcePack.Dispose();
		}
	}
}
