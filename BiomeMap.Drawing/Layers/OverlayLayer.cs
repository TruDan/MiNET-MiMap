using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Drawing.Renderers;
using BiomeMap.Drawing.Renderers.Base;
using BiomeMap.Drawing.Renderers.Overlay;
using BiomeMap.Shared.Configuration;

namespace BiomeMap.Drawing.Layers
{
    public class OverlayLayer : BaseLayer
    {

        public BiomeMapLevelLayerConfig Config { get; }


        public OverlayLayer(LevelMap map, BiomeMapLevelLayerConfig config) : base(map, Path.Combine(map.TilesDirectory, config.LayerId), GetLayerRenderer(config.Renderer), map.Config.LevelId + "_" + config.LayerId)
        {
            Config = config;
            BlendMode = config.BlendMode;
        }

        private static ILayerRenderer GetLayerRenderer(BiomeMapOverlayRenderer overlayRenderer)
        {
            ILayerRenderer renderer = null;
            Console.WriteLine(overlayRenderer);

            switch (overlayRenderer)
            {
                //case BiomeMapOverlayRenderer.BiomeFoilage:

                //    break;
                //case BiomeMapOverlayRenderer.BiomeTemperature:

                //    break;
                case BiomeMapOverlayRenderer.HeightMap:
                    renderer = new HeightMapOverlayRenderer();
                    break;
                case BiomeMapOverlayRenderer.Biome:
                    renderer = new BiomeOverlayRenderer();
                    break;

                default:
                    renderer = new DefaultLayerRenderer();
                    break;
            }

            return renderer;
        }
    }
}
