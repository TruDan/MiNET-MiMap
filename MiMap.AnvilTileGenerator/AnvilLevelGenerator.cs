using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.AnvilTileGenerator.Worlds.Anvil;
using MiNET.Utils;

namespace MiMap.AnvilTileGenerator
{
    public class AnvilLevelGenerator
    {
    
        private string WorldDirectory { get; }

        private AnvilWorldProvider WorldProvider { get; }

        public AnvilLevelGenerator(string worldDirectory)
        {
            WorldDirectory = worldDirectory;
            WorldProvider = new AnvilWorldProvider(worldDirectory);
        }

        public void Process()
        {
            WorldProvider.Initialize();

            var regions = WorldProvider.Regions;
            Parallel.ForEach(regions, LoadRegion);
        }

        private void LoadRegion(ChunkCoordinates regionCoords)
        {
            var region = WorldProvider.GetRegion(regionCoords);
            region.LoadAllChunks();


        }

    }
}
