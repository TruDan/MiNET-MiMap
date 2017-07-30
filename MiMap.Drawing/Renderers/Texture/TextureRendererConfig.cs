using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiMap.Common.Configuration;

namespace MiMap.Drawing.Renderers.Texture
{
    public class TextureRendererConfig : MiMapRendererConfig
    {

        public string ResourcePack { get; set; } = "default";

    }
}
