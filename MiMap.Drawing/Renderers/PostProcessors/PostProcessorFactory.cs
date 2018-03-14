using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MiMap.Drawing.Renderers.Base;
using MiMap.Drawing.Renderers.Overlay;

namespace MiMap.Drawing.Renderers.PostProcessors
{
    public static class PostProcessorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RendererFactory));

        private static readonly IDictionary<string, Type> PostProcessorTypes = new Dictionary<string, Type>();

        static PostProcessorFactory()
        {
            // Register Defaults
            RegisterRenderer("Lighting", typeof(LightingPostProcessor));
            RegisterRenderer("HeightShadow", typeof(HeightShadowPostProcessor));
            RegisterRenderer("DebugGrid", typeof(DebugGridPostProcessor));

        }

        public static void RegisterRenderer(string id, Type type)
        {
            if (!type.IsClass || type.IsGenericType)
            {
                Log.ErrorFormat("Failed to register PostProcessor type {0} - Not valid class or is generic.", type.FullName);
                return;
            }

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var cParams = constructor.GetParameters();
                if (cParams.Length == 0)
                {
                    PostProcessorTypes.Add(id.ToLowerInvariant(), type);
                    return;
                }
                else
                {
                    Log.WarnFormat("Invalid PostProcessor constructor {0}::{1}", type.FullName, string.Join(", ", cParams.Select(p1 => p1.ParameterType.Name)));
                }
            }

            Log.ErrorFormat("Failed to register PostProcessor type {0} - No Valid constructor", type.FullName);
        }

        public static IPostProcessor CreatePostProcessor(string name)
        {
            Type type;
            if (PostProcessorTypes.TryGetValue(name.ToLowerInvariant(), out type))
            {
                return Activator.CreateInstance(type) as IPostProcessor;

            }
            return null;
        }
    }
}
