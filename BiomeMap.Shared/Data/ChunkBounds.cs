namespace BiomeMap.Drawing.Data
{
    public class ChunkBounds : Bounds<ChunkPosition>
    {
        public ChunkBounds(ChunkPosition min, ChunkPosition max) : base(min, max)
        {
        }
    }
}
