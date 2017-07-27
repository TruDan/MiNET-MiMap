using MiMap.Common.Data;
using MiMap.Drawing.Renderers;

namespace MiMap.Drawing.Layers
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
