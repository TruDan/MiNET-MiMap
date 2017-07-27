using MiMap.Common.Configuration;

namespace MiMap.Common.Net
{
    public class MapConfigPacket : IPacket
    {
        public byte Id { get; } = Protocol.MapConfig;

        public MiMapConfig Config { get; set; }
    }
}
