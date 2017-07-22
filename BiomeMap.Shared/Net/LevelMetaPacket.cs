using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Drawing;

namespace BiomeMap.Shared.Net
{
    public class LevelMetaPacket : IPacket
    {

        public byte Id { get; } = 3;


        public string LevelId { get; set; }

        public LevelMeta Meta { get; set; }

    }
}
