using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Common.Data;
using BiomeMap.Drawing.Renderers;
using Size = BiomeMap.Common.Data.Size;

namespace BiomeMap.Drawing.Layers
{
    public interface IMapLayer
    {
        string Directory { get; }

        LevelMap Map { get; }

        ILayerRenderer Renderer { get; }

        IPostProcessor[] PostProcessors { get; }

        void ProcessUpdate();

        void UpdateBlockColumn(BlockColumnMeta column);
    }
}
