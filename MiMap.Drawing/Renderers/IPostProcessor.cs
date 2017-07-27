using System.Drawing;
using MiMap.Common.Data;

namespace MiMap.Drawing.Renderers
{
    public interface IPostProcessor
    {

        void PostProcess(MapRegionLayer layer, Graphics graphics, BlockColumnMeta block);

    }
}
