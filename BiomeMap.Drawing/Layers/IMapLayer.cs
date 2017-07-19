using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Shared.Data;

namespace BiomeMap.Drawing.Layers
{
    public interface IMapLayer
    {
        string Directory { get; }

        LevelMap Map { get; }

        ILayerRenderer Renderer { get; }

        Color Background { get; }

        void ProcessUpdate();

        void UpdateBlockColumn(BlockColumnMeta column);
    }
}
