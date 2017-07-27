namespace MiMap.Common.Data
{
    public class TileBounds : Bounds<TilePosition>
    {
        public TileBounds(TilePosition min, TilePosition max) : base(min, max)
        {
        }
    }
}
