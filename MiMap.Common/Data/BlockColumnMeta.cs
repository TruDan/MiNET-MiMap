namespace MiMap.Common.Data
{
    public class BlockColumnMeta
    {

        public BlockPosition Position { get; set; }

        public int BlockId { get; set; }

        public byte BlockMeta { get; set; }

        public byte Height { get; set; }

        public byte BiomeId { get; set; }

        public int LightLevel { get; set; }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
