using System;

namespace MiMap.Drawing.Utils
{
    public static class MathUtils
    {

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


    }
}
