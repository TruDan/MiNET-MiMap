namespace MiMap.Common.Data
{
    public class ChunkBounds : Bounds<ChunkPosition>
    {
        public ChunkBounds(ChunkPosition min, ChunkPosition max) : base(min, max)
        {
        }
    }
}
