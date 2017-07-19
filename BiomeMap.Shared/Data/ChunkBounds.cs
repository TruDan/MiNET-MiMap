using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Shared.Data
{
    public class ChunkBounds : Bounds<ChunkPosition>
    {
        public ChunkBounds(ChunkPosition min, ChunkPosition max) : base(min, max)
        {
        }
    }
}
