using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MiMap.Common;
using MiMap.Common.Configuration;
using MiMap.Drawing.Renderers.Base;
using MiMap.Drawing.Renderers.Overlay;
using MiMap.Drawing.Renderers.ResourcePack;

namespace MiMap.Drawing.Renderers
{
    public static class RendererFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RendererFactory));

        private static readonly IDictionary<string, RendererDefinition> RendererTypes = new Dictionary<string, RendererDefinition>();

        static RendererFactory()
        {
            // Register Defaults

            // Base Types
            RegisterRenderer("Default", typeof(DefaultLayerRenderer));
            RegisterRenderer("Texture", typeof(TextureLayerRenderer));
	        RegisterRenderer("ResourcePack", typeof(ResourcePackLayerRenderer));

			// Overlay Types

			RegisterRenderer("Biome", typeof(BiomeOverlayRenderer));
            RegisterRenderer("HeightMap", typeof(HeightMapOverlayRenderer));
        }

        public static void RegisterRenderer(string id, Type type)
        {
            if (!type.IsClass || type.IsGenericType)
            {
                Log.ErrorFormat("Failed to register Renderer type {0} - Not valid class or is generic.", type.FullName);
                return;
            }

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var cParams = constructor.GetParameters();
                if (cParams.Length == 0)
                {
                    var entry = new RendererDefinition(type);

                    RendererTypes.Add(id.ToLowerInvariant(), entry);
                    return;
                }
                if (cParams.Length == 1)
                {
                    var p = cParams.First();
                    if (p.ParameterType.IsSubclassOf(typeof(MiMapRendererConfig)))
                    {
                        var entry = new RendererDefinition(type, p.ParameterType);

                        RendererTypes.Add(id.ToLowerInvariant(), entry);
                        return;
                    }
                    else
                    {
                        Log.WarnFormat("Invalid Renderer constructor param {0}::{1} ({2})", type.FullName, string.Join(", ", cParams.Select(p1 => p1.ParameterType.Name)), p.Name);
                    }
                }
                else
                {
                    Log.WarnFormat("Invalid Renderer constructor {0}::{1}", type.FullName, string.Join(", ", cParams.Select(p1 => p1.ParameterType.Name)));
                }
            }

            Log.ErrorFormat("Failed to register Renderer type {0} - No Valid constructor", type.FullName);
        }

        public static ILayerRenderer CreateLayerRenderer(MiMapRendererConfig config)
        {
            RendererDefinition def;
            if (RendererTypes.TryGetValue(config.Type.ToLowerInvariant(), out def))
            {
                return def.Create(config);
            }
            return null;
        }

        private class RendererDefinition
        {
            public Type RendererType { get; }

            public Type ConfigType { get; }

            public RendererDefinition(Type rendererType, Type configType)
            {
                RendererType = rendererType;
                ConfigType = configType;
            }

            public RendererDefinition(Type rendererType) : this(rendererType, null)
            {

            }

            public ILayerRenderer Create(MiMapRendererConfig config)
            {
                if (ConfigType == null)
                {
                    return Activator.CreateInstance(RendererType) as ILayerRenderer;
                }
                else
                {
                    var c = MiMapJsonConvert.DeserializeObject(MiMapJsonConvert.SerializeObject(config), ConfigType);

                    return Activator.CreateInstance(RendererType, c) as ILayerRenderer;
                }
            }
        }
    }
}
