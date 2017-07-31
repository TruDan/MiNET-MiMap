using System;
using System.Net;
using System.Security.Principal;
using log4net;
using Microsoft.Owin.Hosting;
using MiMap.Common.Configuration;
using MiMap.Web.Sockets;
using MiMap.Web.Utils;
using WebSocketProxy;

namespace MiMap.Web
{
    public class MiMapWebServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapWebServer));

        private readonly TcpProxyServer _proxy;
        private IDisposable _host;

        private short PublicPort { get; }
        private short PrivateWebPort { get; }
        private short PrivateWebSocketPort { get; }

        public string Url { get; }
        public MiMapWebServer() : this(new MiMapWebServerConfig()) { }

        public MiMapWebServer(MiMapWebServerConfig config) : this(config.Port, config.InternalWebPort, config.InternalWebSocketPort) { }

        public MiMapWebServer(short publicPort) : this(publicPort, (short)(publicPort + 1), (short)(publicPort + 2)) { }

        public MiMapWebServer(short publicPort, short privateWebPort, short privateWebSocketPort)
        {
            PublicPort = publicPort;
            PrivateWebPort = privateWebPort;
            PrivateWebSocketPort = privateWebSocketPort;

            Url = $"http://localhost:{PrivateWebPort}";

            _proxy = new TcpProxyServer(new TcpProxyConfiguration()
            {
                PublicHost = new Host()
                {
                    IpAddress = IPAddress.Any,
                    Port = PublicPort
                },
                HttpHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = PrivateWebPort
                },
                WebSocketHost = new Host()
                {
                    IpAddress = IPAddress.Loopback,
                    Port = PrivateWebSocketPort
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
            if (!TryStartListener())
            {
                if (!TryAddUrlReservations())
                {
                    return;
                }

                if (!TryStartListener())
                {
                    return;
                }
            }

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

        private bool TryStartListener()
        {
            try
            {
                _host = WebApp.Start<WebStartup>(new StartOptions(Url)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                });
                return true;
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5) // 5 == AccessDenied
                {
                    return false;
                }

                throw;
            }
        }

        private bool TryAddUrlReservations()
        {
            return NetSh.AddUrlAcl(Url, WindowsIdentity.GetCurrent().Name);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
