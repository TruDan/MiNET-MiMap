namespace BiomeMap.Common.Net
{
    public class LevelMetaPacket : IPacket
    {

        public byte Id { get; } = Protocol.LevelMeta;


        public string LevelId { get; set; }

        public LevelMeta Meta { get; set; }

    }
}
