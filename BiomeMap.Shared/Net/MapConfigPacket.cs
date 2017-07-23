using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Shared.Configuration;

namespace BiomeMap.Shared.Net
{
    public class MapConfigPacket : IPacket
    {
        public byte Id { get; } = Protocol.MapConfig;

        public BiomeMapConfig Config { get; set; }
    }
}
