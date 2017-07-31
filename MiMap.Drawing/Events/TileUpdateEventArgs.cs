using MiMap.Common.Data;

namespace MiMap.Drawing.Events
{
    public class TileUpdateEventArgs
    {
        public string LevelId { get; set; }
        public string LayerId { get; }
        public int TileX { get; }
        public int TileY { get; }
        public int TileZoom { get; }

        public TileUpdateEventArgs(string levelId, string layerId, TilePosition tilePos)
        {
            LevelId = levelId;
            LayerId = layerId;
            TileX = tilePos.X;
            TileY = tilePos.Z;
            TileZoom = tilePos.Zoom;
        }
    }
}
