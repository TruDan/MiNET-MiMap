using System.Collections.Generic;

namespace MiMap.Common.Data
{
    public class TilePositionComparer : IEqualityComparer<TilePosition>
    {
        public bool Equals(TilePosition x, TilePosition y)
        {
            return (x == null && y == null) || (x != null && y != null && x.Zoom == y.Zoom && x.X == y.X && x.Z == y.Z);
        }

        public int GetHashCode(TilePosition obj)
        {
            unchecked
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ obj.Zoom.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.X.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Z.GetHashCode();
                return hashCode;
            }
        }
    }
}
