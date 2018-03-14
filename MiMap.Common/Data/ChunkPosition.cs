namespace MiMap.Common.Data
{
    public class ChunkPosition : Position
    {
        public ChunkPosition(int x, int z) : base(x, z)
        {
        }

        public BlockBounds GetBlockBounds()
        {
            return new BlockBounds(
                new BlockPosition(X << 4, Z << 4),
                new BlockPosition(((X + 1) << 4) - 1, ((Z + 1) << 4) - 1)
            );
        }

        public RegionPosition GetRegionPosition()
        {
            return new RegionPosition(X >> 5, Z >> 5);
        }
    }
}