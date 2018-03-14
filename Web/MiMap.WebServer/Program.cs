using System;
using System.IO;
using log4net;
using log4net.Config;
using MiMap.Common.Configuration;
using MiMap.Web;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
namespace MiMap.WebServer
{
    class Program
	{
		private MiMapWebServer _webServer;

		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
			MiMapConfig.Config.TilesDirectory = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location),
				"..", "..", "..", "MiNET.Server", "bin", "Debug", "MiMap", "tiles");

			using (var webServer = new MiMapWebServer())
			{
				webServer.Start();
				Log.InfoFormat("MiMap Web Server Started");
				Console.ReadLine();
			}

			Log.InfoFormat("MiMap Web Server Stopped");
			Console.ReadLine();
		}
	}
}
