namespace MiMap.Common.Net
{
    public class LevelMetaPacket : IPacket
    {

        public int Id { get; } = Protocol.LevelMeta;


        public string LevelId { get; set; }

        public LevelMeta Meta { get; set; }

    }
}
