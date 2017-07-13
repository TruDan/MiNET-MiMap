using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using log4net;
using Nancy.Hosting.Self;
using WebSocketProxy;

namespace BiomeMap.Http
{
    public class BiomeMapWebServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapWebServer));

        private readonly TcpProxyServer _proxy;
        private readonly NancyHost _host;
        private readonly WebSocketServer _server;

        public BiomeMapWebServer()
        {
            _proxy = new TcpProxyServer(new TcpProxyConfiguration()
            {
                PublicHost = new Host()
                {
                    IpAddress = IPAddress.Any,
                    Port = 8123
                },
                HttpHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = 8124
                },
                WebSocketHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = 8125
                }
            });

            _host = new NancyHost(new Uri("http://localhost:8124"));
            _server = new WebSocketServer("ws://0.0.0.0:8125");
        }

        public void Start()
        {
            _host.Start();
            _server.Start(c =>
            {

                c.OnOpen = () => BiomeMapSocketServer.OnOpen(c);
                c.OnClose = () => BiomeMapSocketServer.OnClose(c);
                c.OnMessage = (m) => BiomeMapSocketServer.OnMessage(c,m);

            });
            _proxy.Start();
            Log.InfoFormat("Web server started.");
        }

        public void Stop()
        {
            
            _host.Stop();
            _server.Dispose();
            _proxy.Dispose();
            Log.InfoFormat("Web server stopped.");
        }

    }
}
