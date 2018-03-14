using System;

namespace MiMap.Common.Data
{
    public class TilePosition : Position, IEquatable<TilePosition>
    {
        public int Zoom { get; set; }

        public TilePosition(int x, int z, int zoom) : base(x, z)
        {
            Zoom = zoom;
        }

        public RegionPosition GetRegionPosition()
        {
            return new RegionPosition(X >> Zoom, -(Z >> Zoom));
        }

        public ChunkBounds GetChunkBounds()
        {
            var min = new ChunkPosition((X << 5) >> Zoom, (Z << 5) >> Zoom);
            var size = (1 << 5) >> Zoom;
            return new ChunkBounds(
                min,
                new ChunkPosition(min.X + size, min.Z + size)
                );
        }

        public BlockBounds GetBlockBounds()
        {
            var min = new BlockPosition((X << 9) >> Zoom, (Z << 9) >> Zoom);
            var size = (1 << 9) >> Zoom;
            return new BlockBounds(
                min,
                new BlockPosition(min.X + size, min.Z + size)
            );
        }

        public bool Equals(TilePosition other)
        {
            return base.Equals(other) && Zoom == other.Zoom;
        }
    }
}
