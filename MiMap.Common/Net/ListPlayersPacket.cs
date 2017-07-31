using MiMap.Common.Net.Data;

namespace MiMap.Common.Net
{
    public class ListPlayersPacket : IPacket
    {
        public int Id { get; } = Protocol.ListPlayers;
        public Player[] Players { get; set; }
    }
}
