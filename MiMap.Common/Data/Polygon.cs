namespace MiMap.Common.Data
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
