using System;
using System.IO;
using MiMap.Common.Configuration;
using MiMap.Drawing.Renderers;
using MiMap.Drawing.Renderers.Base;
using MiMap.Drawing.Renderers.Overlay;

namespace MiMap.Drawing.Layers
{
    public class OverlayLayer : BaseLayer
    {

        public MiMapLevelLayerConfig Config { get; }


        public OverlayLayer(LevelMap map, MiMapLevelLayerConfig config) : base(map, Path.Combine(map.TilesDirectory, config.LayerId), GetLayerRenderer(config.Renderer), map.Config.LevelId + "_" + config.LayerId)
        {
            Config = config;
            BlendMode = config.BlendMode;
        }

        private static ILayerRenderer GetLayerRenderer(MiMapOverlayRenderer overlayRenderer)
        {
            ILayerRenderer renderer = null;
            Console.WriteLine(overlayRenderer);

            switch (overlayRenderer)
            {
                //case MiMapOverlayRenderer.BiomeFoilage:

                //    break;
                //case MiMapOverlayRenderer.BiomeTemperature:

                //    break;
                case MiMapOverlayRenderer.HeightMap:
                    renderer = new HeightMapOverlayRenderer();
                    break;
                case MiMapOverlayRenderer.Biome:
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
