using MiMap.Common.Net.Data;

namespace MiMap.Common.Net
{
    public class TileUpdatePacket : IPacket
    {
        public int Id { get; } = Protocol.TileUpdate;

        public string LayerId { get; set; }

        public Tile Tile { get; set; }

    }
}
