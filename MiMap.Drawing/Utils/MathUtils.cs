using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace MiMap.Drawing.Utils
{
    public static class MathUtils
    {
        public static double ToRadians(double deg)
        {
            return deg / 180d * Math.PI;
        }
        
        public static int Clamp(int val, int min, int max)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        public static float Clamp(float val, float min, float max)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        public static double Clamp(double val, double min, double max)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        public static long Clamp(long val, long min, long max)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        private static Random Random { get; } = new Random((int)DateTime.UtcNow.Ticks);

        public static TValue SelectRandomItem<TValue>([NotNull] this IEnumerable<TValue> enumerable)
        {
            var items = enumerable.ToArray();
            return items[Random.Next(items.Length)];

        }

    }
}
