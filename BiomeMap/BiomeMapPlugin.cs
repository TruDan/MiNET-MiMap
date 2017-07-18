using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Http;
using BiomeMap.Output;
using log4net;
using MiNET;
using MiNET.Net;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Utils;
using MiNET.Worlds;

namespace BiomeMap
{

    [Plugin(PluginName = "BiomeMapManager")]
    public class BiomeMapPlugin : Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapPlugin));

        private static BiomeMapPlugin _instance;

        private readonly Dictionary<string, BiomeMapLevelHandler> _handlers = new Dictionary<string, BiomeMapLevelHandler>();

        private BiomeMapWebServer _webServer;

        public BiomeMapPlugin()
        {
            _instance = this;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _webServer = new BiomeMapWebServer();
            _webServer.Start();

            Context.LevelManager.LevelCreated += LevelManagerOnLevelCreated;
            Context.Server.PlayerFactory.PlayerCreated += PlayerFactoryOnPlayerCreated;
        }

        private void PlayerFactoryOnPlayerCreated(object sender, PlayerEventArgs playerEventArgs)
        {
            playerEventArgs.Player.PlayerJoin += PlayerOnPlayerJoin;
            playerEventArgs.Player.PlayerLeave += PlayerOnPlayerLeave;
        }

        private void PlayerOnPlayerLeave(object sender, PlayerEventArgs playerEventArgs)
        {
            BiomeMapSocketServer.OnPlayerQuit(playerEventArgs.Player);
        }

        private void PlayerOnPlayerJoin(object sender, PlayerEventArgs playerEventArgs)
        {
            BiomeMapSocketServer.OnPlayerJoin(playerEventArgs.Player);
        }
        
        [PacketHandler, Receive]
        public void OnPlayerMove(McpeMovePlayer packet, Player player)
        {
            BiomeMapSocketServer.OnPlayerMove(player);
        }

        private void LevelManagerOnLevelCreated(object sender, LevelEventArgs levelEventArgs)
        {
            var h = new BiomeMapLevelHandler(levelEventArgs.Level);
            _handlers.Add(levelEventArgs.Level.LevelId, h);

            Log.InfoFormat("Started handler for level {0}", levelEventArgs.Level.LevelId);
            h.Start();

            var chunkGen = Config.GetProperty("GenChunks", 64);
            if (chunkGen > 0)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    var level = levelEventArgs.Level;
                    
                    for (int i = 0; i < chunkGen; i++)
                    {
                        GenerateChunks(level, i);
                    }
                });
            }
        }

        public static BiomeMapLevelHandler GetMapHandler(string levelId)
        {
            BiomeMapLevelHandler handler;
            _instance._handlers.TryGetValue(levelId, out handler);

            return handler;
        }

        public static string[] GetLevelNames()
        {
            return _instance._handlers.Keys.ToArray();
        }

        private void GenerateChunks(Level level, int r)
        {
            Log.InfoFormat("Generating chunks for radius {0}", r);
            for (var x = -r; x <= r; x++)
            {
                for (var z = -r; z <= r; z++)
                {
                    if(x > -r && x < r && z >-r && z < r)
                        continue;
                    
                    level.GetChunk(new ChunkCoordinates(x, z));
                }
            }
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
