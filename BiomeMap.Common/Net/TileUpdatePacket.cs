using BiomeMap.Common.Net.Data;

namespace BiomeMap.Common.Net
{
    public class TileUpdatePacket : IPacket
    {
        public byte Id { get; } = Protocol.TileUpdate;

        public string LayerId { get; set; }

        public Tile Tile { get; set; }

    }
}
