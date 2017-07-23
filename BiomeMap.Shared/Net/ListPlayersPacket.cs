using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Shared.Net.Data;

namespace BiomeMap.Shared.Net
{
    public class ListPlayersPacket : IPacket
    {
        public byte Id { get; } = Protocol.ListPlayers;
        public Player[] Players { get; set; }
    }
}
