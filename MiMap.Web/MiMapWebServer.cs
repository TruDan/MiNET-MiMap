using System;
using System.Net;
using log4net;
using Microsoft.Owin.Hosting;
using MiMap.Web.Sockets;
using WebSocketProxy;

namespace MiMap.Web
{
    public class MiMapWebServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapWebServer));

        private readonly TcpProxyServer _proxy;
        private IDisposable _host;

        public MiMapWebServer()
        {
            _proxy = new TcpProxyServer(new TcpProxyConfiguration()
            {
                PublicHost = new Host()
                {
                    IpAddress = IPAddress.Any,
                    Port = 8124
                },
                HttpHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = 8125
                },
                WebSocketHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = 8126
                }
            });


            /*
            _host = new NancyHost(new Uri("http://localhost:8125"), new WebBootstrapper(), new HostConfiguration()
            {
                UrlReservations = new UrlReservations()
                {
                    CreateAutomatically = true
                }
            });*/
        }

        public void Start()
        {
            _host = WebApp.Start<WebStartup>("http://+:8125");
            WsServer.Start();
            _proxy.Start();
            Log.InfoFormat("Web server started.");
        }

        public void Stop()
        {

            _host.Dispose();
            WsServer.Stop();
            _proxy.Dispose();
            Log.InfoFormat("Web server stopped.");
        }

    }
}
