using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Shared.Net.Data;

namespace BiomeMap.Shared.Net
{
    public class TileUpdatePacket : IPacket
    {
        public byte Id { get; } = Protocol.TileUpdate;

        public string LayerId { get; set; }

        public Tile Tile { get; set; }

    }
}
