using BiomeMap.Common.Net.Data;

namespace BiomeMap.Common.Net
{
    public class ListPlayersPacket : IPacket
    {
        public byte Id { get; } = Protocol.ListPlayers;
        public Player[] Players { get; set; }
    }
}
