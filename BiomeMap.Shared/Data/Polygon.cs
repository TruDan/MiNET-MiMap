using System;
using System.Collections.Generic;
using System.Text;
using BiomeMap.Drawing.Data;

namespace BiomeMap.Shared.Data
{
    public class Polygon
    {
        public Position[] Points { get; private set; }


        public Polygon(Position[] points)
        {
            Points = points;
        }

    }
}
