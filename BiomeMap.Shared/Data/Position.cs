using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Shared.Data
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

        public override bool Equals(object obj)
        {
            var b = obj as Position;
            if (b != null)
            {
                return X == b.X && Z == b.Z;
            }

            return base.Equals(obj);
        }
    }
}
