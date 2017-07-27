using System;

namespace MiMap.Common.Data
{
    public abstract class Bounds<TPositionType> where TPositionType : Position
    {
        public TPositionType Min { get; set; }
        public TPositionType Max { get; set; }

        public int Width => Math.Abs(Max.X - Min.X);
        public int Height => Math.Abs(Max.Z - Min.Z);

        protected Bounds(TPositionType min, TPositionType max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(TPositionType position)
        {
            return position.X >= Min.X && position.X <= Max.X &&
                   position.Z >= Min.Z && position.Z <= Max.Z;
        }
    }
}
