namespace BiomeMap.Common.Data
{
    public class BlockBounds : Bounds<BlockPosition>
    {
        public BlockBounds(BlockPosition min, BlockPosition max) : base(min, max)
        {
        }
    }
}
