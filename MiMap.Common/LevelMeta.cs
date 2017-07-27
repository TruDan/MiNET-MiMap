using System;
using MiMap.Common.Data;

namespace MiMap.Common
{
    public class LevelMeta
    {
        public string Id { get; set; }

        public string Name { get; set; }


        public int MinZoom { get; set; }

        public int MaxZoom { get; set; }


        public Size TileSize { get; set; }

        public BlockBounds Bounds { get; set; }

        public Size Size { get; set; }


        public DateTime LastUpdate { get; set; }
    }
}
