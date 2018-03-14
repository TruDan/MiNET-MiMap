using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Drawing.Utils
{
    public static class GeometryUtils
    {

        public static PointF GetCenter(this RectangleF rectangle)
        {
            return new PointF(rectangle.Left + (rectangle.Width / 2f), rectangle.Bottom + (rectangle.Height / 2f));
        }
        public static Point GetCenter(this Rectangle rectangle)
        {
            return new Point(rectangle.Left + (int)(rectangle.Width / 2f), rectangle.Bottom + (int)(rectangle.Height / 2f));
        }

    }
}
