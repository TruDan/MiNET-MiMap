using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using log4net;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Nancy;
using Nancy.Hosting.Self;
using WebSocketProxy;

namespace BiomeMap.Plugin.Net
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
