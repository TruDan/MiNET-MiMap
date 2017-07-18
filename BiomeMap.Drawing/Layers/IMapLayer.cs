using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Data;
using BiomeMap.Drawing.Renderers;

namespace BiomeMap.Drawing.Layers
{
    public interface IMapLayer
    {
        string Directory { get; }

        LevelMap Map { get; }

        ILayerRenderer Renderer { get; }

        void ProcessUpdate();

        void UpdateBlockColumn(BlockColumnMeta column);
    }
}
