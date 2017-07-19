namespace BiomeMap.Drawing.Data
{
    public class BlockBounds : Bounds<BlockPosition>
    {
        public BlockBounds(BlockPosition min, BlockPosition max) : base(min, max)
        {
        }
    }
}
