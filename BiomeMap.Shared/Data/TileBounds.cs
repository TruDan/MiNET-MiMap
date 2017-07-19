namespace BiomeMap.Drawing.Data
{
    public class TileBounds : Bounds<TilePosition>
    {
        public TileBounds(TilePosition min, TilePosition max) : base(min, max)
        {
        }
    }
}
