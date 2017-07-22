using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Shared.Configuration;

namespace BiomeMap.Shared.Net
{
    public class MapConfigPacket : IPacket
    {
        public byte Id { get; } = 1;

        public BiomeMapConfig Config { get; set; }
    }
}
