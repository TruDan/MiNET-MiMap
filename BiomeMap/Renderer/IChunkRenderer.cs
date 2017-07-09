using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Data;
using MiNET.Worlds;

namespace BiomeMap.Renderer
{
    public interface IChunkRenderer
    {

        ChunkData RenderChunk(ChunkColumn chunk);

    }
}
