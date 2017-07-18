using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Drawing.Data
{
    public class TilePosition : Position
    {
        public int Zoom { get; set; }

        public TilePosition(int x, int z, int zoom) : base(x, z)
        {
        }

        public RegionPosition GetRegionPosition()
        {
            return new RegionPosition(X >> Zoom, Z >> Zoom);
        }

        public ChunkBounds GetChunkBounds()
        {
            var regionPos = GetRegionPosition();
            return new ChunkBounds(
                new ChunkPosition((regionPos.X << 5) + ((X % (1 << Zoom)) << 5), (regionPos.Z << 5) + ((Z % (1 << Zoom)) << 5)),
                new ChunkPosition((regionPos.X << 5) + (((X % (1 << Zoom)) + 1) << 5), (regionPos.Z << 5) + (((Z % (1 << Zoom))+1) << 5))
                );
        }
        public BlockBounds GetBlockBounds()
        {
            var regionPos = GetRegionPosition();
            return new BlockBounds(
                new BlockPosition((regionPos.X << 9) + ((X % (1 << Zoom)) << 9), (regionPos.Z << 9) + ((Z % (1 << Zoom)) << 9)),
                new BlockPosition((regionPos.X << 9) + (((X % (1 << Zoom)) + 1) << 9), (regionPos.Z << 9) + (((Z % (1 << Zoom)) + 1) << 9))
            );
        }
    }
}
