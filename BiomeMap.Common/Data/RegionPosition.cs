
namespace BiomeMap.Common.Data
{
    public class RegionPosition : Position
    {
        public RegionPosition(int x, int z) : base(x, z)
        {
        }

        public BlockBounds GetBlockBounds()
        {
            return new BlockBounds(
                new BlockPosition(X << 9, Z << 9),
                new BlockPosition((X + 1) << 9, (Z + 1) << 9)
            );
        }

        public ChunkBounds GetChunkBounds()
        {
            return new ChunkBounds(
                new ChunkPosition(X << 5, Z << 5),
                new ChunkPosition((X + 1) << 5, (Z + 1) << 5)
                );
        }
    }
}
