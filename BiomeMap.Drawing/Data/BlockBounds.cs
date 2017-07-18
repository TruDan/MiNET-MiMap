using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Drawing.Data
{
    public class BlockBounds : Bounds<BlockPosition>
    {
        public BlockBounds(BlockPosition min, BlockPosition max) : base(min, max)
        {
        }
    }
}
