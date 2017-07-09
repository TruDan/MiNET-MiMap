using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Http;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Worlds;

namespace BiomeMap
{

    [Plugin(PluginName = "BiomeMap")]
    public class BiomeMapPlugin : Plugin
    {
        private Dictionary<string, BiomeMapLevelHandler> _handlers = new Dictionary<string, BiomeMapLevelHandler>();

        private BiomeMapWebServer _webServer;

        protected override void OnEnable()
        {
            base.OnEnable();

            _webServer = new BiomeMapWebServer();
            _webServer.Start();
            Context.LevelManager.LevelCreated += LevelManagerOnLevelCreated;
        }

        private void LevelManagerOnLevelCreated(object sender, LevelEventArgs levelEventArgs)
        {
            var h = new BiomeMapLevelHandler(levelEventArgs.Level);
            _handlers.Add(levelEventArgs.Level.LevelId, h);

            h.Start();
        }

        public override void OnDisable()
        {
            _webServer.Stop();

            foreach (var h in _handlers.Values.ToArray())
            {
                h.Stop();
            }
            base.OnDisable();

        }
    }
}
