using BiomeMap.Common.Configuration;

namespace BiomeMap.Common.Net
{
    public class MapConfigPacket : IPacket
    {
        public byte Id { get; } = Protocol.MapConfig;

        public BiomeMapConfig Config { get; set; }
    }
}
