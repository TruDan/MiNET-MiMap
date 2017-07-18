using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Drawing.Data
{
    public abstract class Position
    {
        public int X { get; set; }
        public int Z { get; set; }

        protected Position(int x, int z)
        {
            X = x;
            Z = z;
        }

        public override string ToString()
        {
            return $"{X},{Z}";
        }
    }
}
