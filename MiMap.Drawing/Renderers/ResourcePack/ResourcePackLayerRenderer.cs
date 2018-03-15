using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;
using MiMap.Common.Data;
using MiMap.Drawing.Properties;
using MiMap.Drawing.Utils;
using MiMap.ResourcePackLib;
using MiMap.ResourcePackLib.Json;
using MiMap.ResourcePackLib.Json.BlockStates;
using MiMap.ResourcePackLib.Json.Models;
using MiNET.Blocks;
using MiNET.Worlds;
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
				ResourcePack = new MinecraftResourcePack(new MemoryStream(Resources._default));
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

					//	Bitmap texture;
					bool doTint = false;
					Bitmap faceTexture = null;
					int x = 0, y = 0, width = 16, height = 16;
					if (modelElement.Faces.TryGetValue(BlockFace.Up, out var face))
					{
						doTint = face.TintIndex > 0;
						if (ResourcePack.TryGetTexture(model, face.TextureName, out faceTexture))
						{
							x = face.UV.X1;
							y = face.UV.Y1;
							width = face.UV.X2 - face.UV.X1;
							height = face.UV.Y2 - face.UV.Y1;
						}
					}

					if (faceTexture == null)
					{
						var str = model.TextureDefinitions.FirstOrDefault();
						ResourcePack.TryGetTexture(model, str.Value, out faceTexture);
					}

					if (faceTexture != null)
					{
						var srcRect = new RectangleF(x, y, width, height);
						graphics.DrawImage(faceTexture, rect, srcRect, GraphicsUnit.Pixel);

						if (doTint)
						{
							var biome = BiomeUtils.GetBiome(blockColumn.BiomeId);
							var c = Color.FromArgb(biome.Foliage);
							var tint = Color.FromArgb(128, c.R, c.G, c.B);

							using (SolidBrush solidBrush = new SolidBrush(tint))
							{
								graphics.FillRectangle(solidBrush, rect);
							}
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
