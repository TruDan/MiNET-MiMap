using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;

namespace BiomeMap.Drawing.Events
{
    public class TileUpdateEventArgs
    {
        public string LayerId { get; }
        public int TileX { get; }
        public int TileY { get; }
        public int TileZoom { get; }

        public TileUpdateEventArgs(string layerId, TilePosition tilePos)
        {
            LayerId = layerId;
            TileX = tilePos.X;
            TileY = tilePos.Z;
            TileZoom = tilePos.Zoom;
        }
    }
}
