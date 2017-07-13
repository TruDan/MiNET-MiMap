using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Data;

namespace BiomeMap.Output
{
    public interface IRenderOutput
    {

        void WriteChunk(ChunkData data);

        void OnUpdateStart();
        void OnUpdateEnd();
    }
}
