using MiMap.Common.Configuration;

namespace MiMap.Common.Net
{
    public class MapConfigPacket : IPacket
    {
        public int Id { get; } = Protocol.MapConfig;

        public MiMapConfig Config { get; set; }
    }
}
