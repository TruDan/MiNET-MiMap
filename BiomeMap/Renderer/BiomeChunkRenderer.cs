using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Data;
using MiNET.Worlds;

namespace BiomeMap.Renderer
{
    public class BiomeChunkRenderer : IChunkRenderer
    {
        private readonly BiomeUtils _biomeUtils;

        public BiomeChunkRenderer()
        {
            _biomeUtils = new BiomeUtils();
            _biomeUtils.PrecomputeBiomeColors();
        }

        public ChunkData RenderChunk(ChunkColumn chunk)
        {

            var d = new ChunkData();
            d.X = chunk.x;
            d.Z = chunk.z;

            d.BlockColors = new Color[16 * 16];

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var biomeId = chunk.GetBiome(x, z);
                    var highest = GetHeight(chunk, x, z);

                    d.BlockColors[x * 16 + z] = GetBlockColor(biomeId, highest);
                }
            }

            return d;
        }

        private byte GetHeight(ChunkColumn chunk, int x, int z)
        {
            for (byte y = 0; y < 256; y++)
            {
                if (chunk.GetBlock(x, y, z) > 0)
                {
                    return y;
                }
            }

            return 255;
        }

        private Color GetBlockColor(byte biomeId, byte maxHeight)
        {
            var biomeColor = _biomeUtils.GetBiome(biomeId).Foliage;
            var alphaOverlay = ((maxHeight / 128) - 1) * 0.25;

            var c = Color.FromArgb(biomeColor);

            var r = (int)Math.Min(255, Math.Max(0, ((c.R / 255f) * (0.75 + alphaOverlay)) * 255f));
            var g = (int)Math.Min(255, Math.Max(0, ((c.G / 255f) * (0.75 + alphaOverlay)) * 255f));
            var b = (int)Math.Min(255, Math.Max(0, ((c.B / 255f) * (0.75 + alphaOverlay)) * 255f));

            return Color.FromArgb(255, r, g, b);
        }
    }
}
