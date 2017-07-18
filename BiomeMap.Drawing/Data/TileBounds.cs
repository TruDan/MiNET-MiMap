using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Drawing.Data
{
    public class TileBounds : Bounds<TilePosition>
    {
        public TileBounds(TilePosition min, TilePosition max) : base(min, max)
        {
        }
    }
}
