using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Shared.Net;
using BiomeMap.Shared.Net.Serialization;
using Fleck;
using Google.Protobuf;
using log4net;

namespace BiomeMap.Plugin.Net
{
    public static class WsServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WsServer));


        private static readonly object _sync = new object();
        public static List<IWebSocketConnection> Connections = new List<IWebSocketConnection>();


        private static readonly WebSocketServer _server = new WebSocketServer("ws://0.0.0.0:5001");


        public static void OnOpen(IWebSocketConnection c)
        {
            lock (_sync)
            {
                Connections.Add(c);
                SendPacket(c, new MapConfigPacket()
                {
                    Config = BiomeMapPlugin.Config
                });

                foreach (var levelRunner in BiomeMapPlugin.Instance.LevelRunners)
                {
                    SendPacket(c, new LevelMetaPacket()
                    {
                        LevelId = levelRunner.Map.Meta.Id,
                        Meta = levelRunner.Map.Meta
                    });
                }
            }
        }

        public static void OnClose(IWebSocketConnection c)
        {
            lock (_sync)
            {
                Connections.Remove(c);
            }
        }

        public static void SendPacket(IWebSocketConnection connection, IPacket packet)
        {
            lock (_sync)
            {
                var msg = packet.Encode();
                connection.Send(msg);
            }
        }

        public static void BroadcastPacket(IPacket packet)
        {
            lock (_sync)
            {
                var msg = packet.Encode();

                Parallel.ForEach(Connections, c =>
                {
                    c.Send(msg);
                });
            }
        }

        public static void OnMessage(IWebSocketConnection c, string message)
        {

        }

        public static void Start()
        {

            _server.Start(c =>
            {
                c.OnOpen = () => OnOpen(c);
                c.OnClose = () => OnClose(c);
                c.OnMessage = (m) => OnMessage(c, m);
            });
            Log.InfoFormat("Web server started.");
        }


        public static void Stop()
        {
            _server.Dispose();
            Log.InfoFormat("Web server stopped.");
        }

    }
}
