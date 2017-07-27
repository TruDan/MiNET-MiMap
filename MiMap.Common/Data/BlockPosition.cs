namespace MiMap.Common.Data
{
    public class BlockPosition : Position
    {
        public BlockPosition(int x, int z) : base(x, z)
        {
        }

        public ChunkPosition GetChunkPosition()
        {
            return new ChunkPosition(X >> 4, Z >> 4);
        }

        public RegionPosition GetRegionPosition()
        {
            return new RegionPosition(X >> 9, Z >> 9);
        }

        public override string ToString()
        {
            return $"({X}, {Z})";
        }
    }
}
