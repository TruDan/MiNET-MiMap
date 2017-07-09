using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Nancy.Hosting.Self;

namespace BiomeMap.Http
{
    public class BiomeMapWebServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapWebServer));

        private readonly NancyHost _host;

        public BiomeMapWebServer()
        {
            _host = new NancyHost(new Uri("http://localhost:8123"));

        }

        public void Start()
        {
            _host.Start();
            Log.InfoFormat("Web server started.");
        }

        public void Stop()
        {
            _host.Stop();
            Log.InfoFormat("Web server stopped.");
        }

    }
}
