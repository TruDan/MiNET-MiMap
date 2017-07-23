using System;
using System.Collections.Generic;
using System.Text;

namespace BiomeMap.Shared.Net
{
    public class TileSubscribePacket : IPacket
    {
        public byte Id { get; } = Protocol.TileSubscribe;

        public bool Subscribe { get; set; } = true;

        public int CurrentZoomLevel { get; set; } = 0;

    }
}
