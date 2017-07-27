namespace MiMap.Common.Data
{
    public class Position
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

        protected bool Equals(Position other)
        {
            return X == other.X && Z == other.Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }
    }
}
